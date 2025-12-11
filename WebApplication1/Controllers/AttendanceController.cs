using ClassMate.Api.Data;
using ClassMate.Api.DTOs;
using ClassMate.Api.Entities;
using ClassMate.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassMate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // Giảng viên tạo buổi điểm danh -> sinh Code
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateAttendanceSessionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var cls = await _context.ClassSections.FindAsync(request.ClassSectionId);
            if (cls == null) return BadRequest(new { message = "ClassSection not found" });

            // tạo code ngẫu nhiên (có thể dùng GUID)
            var code = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
            var startUtc = request.StartTime.Kind == DateTimeKind.Utc
        ? request.StartTime
        : DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);

            var endUtc = request.EndTime.Kind == DateTimeKind.Utc
                ? request.EndTime
                : DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc);
            var session = new AttendanceSession
            {
                ClassSectionId = request.ClassSectionId,
                StartTime = startUtc,
                EndTime = endUtc,
                Code = code
            };
            _context.AttendanceSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                session.Id,
                session.ClassSectionId,
                StartTime = session.StartTime.ToVietnamTime(),
                EndTime = session.EndTime.ToVietnamTime(),
                session.Code // frontend lấy code này render QR
            });
        }

        // Sinh viên check-in bằng Code (từ QR)
        [Authorize(Roles = "Student")]
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var now = DateTime.UtcNow;

            var session = await _context.AttendanceSessions
                .FirstOrDefaultAsync(s => s.Code == request.Code);

            if (session == null)
                return BadRequest(new { message = "Invalid code" });

            if (now < session.StartTime || now > session.EndTime)
                return BadRequest(new { message = "Attendance time is over or not started" });

            var enrolled = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.ClassSectionId == session.ClassSectionId);

            if (!enrolled)
                return BadRequest(new { message = "You are not in this class" });

            var exist = await _context.AttendanceRecords
                .AnyAsync(r => r.AttendanceSessionId == session.Id && r.UserId == userId);

            if (exist)
                return BadRequest(new { message = "You have already checked in" });

            var record = new AttendanceRecord
            {
                AttendanceSessionId = session.Id,
                UserId = userId,
                CheckedInAt = now
            };

            _context.AttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Check-in success" });
        }

        // Giảng viên xem lịch sử điểm danh 1 buổi
        [Authorize(Roles = "Teacher,Admin")]
        [HttpGet("sessions/{sessionId:int}/records")]
        public async Task<IActionResult> GetRecords(int sessionId)
        {
            var session = await _context.AttendanceSessions
                .Include(s => s.ClassSection)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound();

            var records = await _context.AttendanceRecords
                .Where(r => r.AttendanceSessionId == sessionId)
                .Include(r => r.User)
                .Select(r => new
                {
                    r.UserId,
                    r.User.FullName,
                    r.User.UserName,
                    r.CheckedInAt
                }).ToListAsync();

            return Ok(new
            {
                session.Id,
                session.ClassSectionId,
                session.Code,
                StartTime = session.StartTime.ToVietnamTime(),
                EndTime = session.EndTime.ToVietnamTime(),
                TotalChecked = records.Count,
                Records = records
            });
        }

        // Sinh viên xem lịch sử điểm danh của mình
        [Authorize(Roles = "Student")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAttendance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var list = await _context.AttendanceRecords
                .Where(r => r.UserId == userId)
                .Include(r => r.AttendanceSession)
                    .ThenInclude(s => s.ClassSection)
                        .ThenInclude(c => c.Course)
                .Select(r => new
                {
                    r.Id,
                    CheckedInAt=r.CheckedInAt.ToVietnamTime(),
                    SessionId = r.AttendanceSessionId,
                    r.AttendanceSession.Code,
                    ClassId = r.AttendanceSession.ClassSectionId,
                    ClassName = r.AttendanceSession.ClassSection.Name,
                    CourseName = r.AttendanceSession.ClassSection.Course.Name
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}
