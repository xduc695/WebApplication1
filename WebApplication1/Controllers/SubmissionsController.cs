using ClassMate.Api.Data;
using ClassMate.Api.DTOs;
using ClassMate.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassMate.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class SubmissionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SubmissionsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================
        // ➤ STUDENT NỘP BÀI
        // ==========================
        [Authorize(Roles = "Student")]
        [HttpPost("assignments/{assignmentId}/submit")]
        public async Task<IActionResult> Submit(
            int assignmentId,
            [FromForm] SubmissionRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var assignment = await _context.Assignments
                .Include(a => a.ClassSection)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound("Assignment not found");

            // ⚠️ Kiểm tra sinh viên đã enroll lớp chưa
            var enrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId &&
                               e.ClassSectionId == assignment.ClassSectionId);

            if (!enrolled)
                return Forbid("Not in this class!");

            string? fileUrl = null;
            if (request.File != null)
            {
                var ext = Path.GetExtension(request.File.FileName).ToLower();

                var allowed = new[] { ".pdf", ".docx", ".zip" };
                if (!allowed.Contains(ext))
                    return BadRequest("Allowed file types: pdf, docx, zip");

                var folder = Path.Combine(_env.ContentRootPath, "Submissions");
                Directory.CreateDirectory(folder);

                var fileName = $"sub_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await request.File.CopyToAsync(stream);

                fileUrl = $"/submissions/{fileName}";
            }

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                UserId = userId,
                AnswerText = request.AnswerText,
                FileUrl = fileUrl,
                SubmittedAt = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Submitted successfully",
                submissionId = submission.Id
            });
        }

        // ==========================
        // ➤ SV XEM LỊCH SỬ NỘP BÀI
        // ==========================
        [Authorize(Roles = "Student")]
        [HttpGet("assignments/{assignmentId}/submissions/me")]
        public async Task<IActionResult> GetMySubmissions(int assignmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var subs = await _context.Submissions
                .Where(s => s.AssignmentId == assignmentId && s.UserId == userId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return Ok(subs.Select(s => new
            {
                s.Id,
                s.AnswerText,
                s.FileUrl,
                s.Score,
                s.Feedback,
                s.SubmittedAt
            }));
        }

        // ==========================
        // ➤ GV XEM TẤT CẢ BÀI NỘP
        // ==========================
        [Authorize(Roles = "Teacher,Admin")]
        [HttpGet("assignments/{assignmentId}/submissions")]
        public async Task<IActionResult> GetClassSubmissions(int assignmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var assignment = await _context.Assignments
                .Include(a => a.ClassSection)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment?.ClassSection.TeacherId != userId)
                return Forbid("You are not the teacher of this class");

            var subs = await _context.Submissions
                .Include(s => s.User)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return Ok(subs.Select(s => new
            {
                s.Id,
                s.UserId,
                studentName = s.User.FullName,
                s.FileUrl,
                s.AnswerText,
                s.Score,
                s.Feedback,
                s.SubmittedAt
            }));
        }

        // ==========================
        // ➤ GV CHẤM ĐIỂM
        // ==========================
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPut("submissions/{submissionId}/grade")]
        public async Task<IActionResult> Grade(int submissionId, [FromBody] GradeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sub = await _context.Submissions
                .Include(s => s.Assignment)
                .ThenInclude(a => a.ClassSection)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (sub == null) return NotFound("Submission not found");

            if (sub.Assignment.ClassSection.TeacherId != userId)
                return Forbid("Not your class");

            sub.Score = request.Score;
            sub.Feedback = request.Feedback;
            await _context.SaveChangesAsync();

            return Ok("Graded successfully");
        }
    }
}
