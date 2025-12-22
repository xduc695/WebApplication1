using ClassMate.Api.Data;
using ClassMate.Api.DTOs;
using ClassMate.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ClassMate.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class AssignmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;

        public AssignmentsController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // ======================
        // TẠO BÀI TẬP (Teacher)
        // ======================
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost("classes/{classId}/assignments")]
        public async Task<IActionResult> CreateAssignment(
            int classId,
            [FromForm] AssignmentCreateRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(
                await _userManager.FindByIdAsync(userId)
            );
            var isAdmin = roles.Contains("Admin");

            var cls = await _context.ClassSections.FindAsync(classId);
            if (cls == null) return NotFound("Class not found.");

            // if not admin, must be the teacher of the class
            if (!isAdmin && cls.TeacherId != userId)
                return Forbid("You are not the teacher of this class.");

            string? attachmentUrl = null;

            if (request.Attachment != null)
            {
                var folder = Path.Combine(_env.ContentRootPath, "Assignments");
                Directory.CreateDirectory(folder);

                var ext = Path.GetExtension(request.Attachment.FileName) ?? ".dat";
                var fileName = $"assign_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Attachment.CopyToAsync(stream);
                }

                attachmentUrl = $"/assignments/{fileName}";
            }

            var assignment = new Assignment
            {
                Title = request.Title,
                Content = request.Content,
                DueDate = request.DueDate,
                ClassSectionId = classId,
                AttachmentUrl = attachmentUrl
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, new AssignmentResponse
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Content = assignment.Content,
                DueDate = assignment.DueDate,
                CreatedAt = assignment.CreatedAt,
                ClassSectionId = classId,
                AttachmentUrl = assignment.AttachmentUrl
            });
        }

        // ======================
        // LẤY DANH SÁCH BÀI TẬP
        // ======================
        [Authorize]
        [HttpGet("classes/{classId}/assignments")]
        public async Task<IActionResult> GetAssignments(int classId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cls = await _context.ClassSections.FindAsync(classId);
            if (cls == null) return NotFound("Class not found.");

            var roles = await _userManager.GetRolesAsync(
                await _userManager.FindByIdAsync(userId)
            );
            var isTeacherOrAdmin = roles.Contains("Teacher") || roles.Contains("Admin");

            if (!isTeacherOrAdmin)
            {
                var enrolled = await _context.Enrollments
                    .AnyAsync(e => e.ClassSectionId == classId && e.UserId == userId);
                if (!enrolled) return Forbid();
            }

            var data = await _context.Assignments
                .Where(a => a.ClassSectionId == classId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AssignmentResponse
                {
                    Id = a.Id,
                    Title = a.Title,
                    Content = a.Content,
                    DueDate = a.DueDate,
                    CreatedAt = a.CreatedAt,
                    ClassSectionId = a.ClassSectionId,
                    AttachmentUrl = a.AttachmentUrl
                })
                .ToListAsync();

            return Ok(data);
        }

        // ======================
        // CHI TIẾT BÀI TẬP
        // ======================
        [Authorize]
        [HttpGet("assignments/{id}")]
        public async Task<IActionResult> GetAssignment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var a = await _context.Assignments.FindAsync(id);
            if (a == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(
                await _userManager.FindByIdAsync(userId)
            );
            var isTeacherOrAdmin = roles.Contains("Teacher") || roles.Contains("Admin");

            if (!isTeacherOrAdmin)
            {
                var enrolled = await _context.Enrollments
                    .AnyAsync(e => e.ClassSectionId == a.ClassSectionId && e.UserId == userId);
                if (!enrolled) return Forbid();
            }

            return Ok(new AssignmentResponse
            {
                Id = a.Id,
                Title = a.Title,
                Content = a.Content,
                DueDate = a.DueDate,
                CreatedAt = a.CreatedAt,
                ClassSectionId = a.ClassSectionId,
                AttachmentUrl = a.AttachmentUrl
            });
        }

        // ======================
        // SỬA BÀI TẬP
        // ======================
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPut("assignments/{id}")]
        public async Task<IActionResult> UpdateAssignment(
            int id,
            [FromForm] AssignmentCreateRequest request)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(
                await _userManager.FindByIdAsync(userId)
            );
            var isAdmin = roles.Contains("Admin");

            var cls = await _context.ClassSections.FindAsync(assignment.ClassSectionId);
            if (cls == null) return NotFound("Class not found.");
            if (!isAdmin && cls.TeacherId != userId)
                return Forbid("Not your class.");

            assignment.Title = request.Title;
            assignment.Content = request.Content;
            assignment.DueDate = request.DueDate;

            if (request.Attachment != null)
            {
                var folder = Path.Combine(_env.ContentRootPath, "Assignments");
                Directory.CreateDirectory(folder);

                var ext = Path.GetExtension(request.Attachment.FileName);
                var fileName = $"assign_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await request.Attachment.CopyToAsync(stream);

                assignment.AttachmentUrl = $"/assignments/{fileName}";
            }

            await _context.SaveChangesAsync();
            return Ok("Updated.");
        }

        // ======================
        // XÓA BÀI TẬP
        // ======================
        [Authorize(Roles = "Teacher,Admin")]
        [HttpDelete("assignments/{id}")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var a = await _context.Assignments.FindAsync(id);
            if (a == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(
                await _userManager.FindByIdAsync(userId)
            );
            var isAdmin = roles.Contains("Admin");

            var cls = await _context.ClassSections.FindAsync(a.ClassSectionId);
            if (cls == null) return NotFound("Class not found.");
            if (!isAdmin && cls.TeacherId != userId)
                return Forbid("Not your class.");

            _context.Assignments.Remove(a);
            await _context.SaveChangesAsync();

            return Ok("Deleted.");
        }
    }
}
