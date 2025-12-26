using ClassMate.Api.Data;
using ClassMate.Api.DTOs;
using ClassMate.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassMate.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class CourseResourcesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CourseResourcesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost("courses/{courseId}/resources")]
        public async Task<IActionResult> UploadResource(int courseId, [FromForm] CourseResourceCreateRequest request)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound("Course không tồn tại");

            var res = new CourseResource
            {
                CourseId = courseId,
                Title = request.Title,
                Description = request.Description,
                LinkUrl = request.LinkUrl,
                ResourceFiles = new List<CourseResourceFile>()
            };

            if (request.Files != null && request.Files.Any())
            {
                // Trỏ vào thư mục 'CourseResources' ở gốc dự án
                var folder = Path.Combine(_env.ContentRootPath, "CourseResources");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                foreach (var file in request.Files)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    res.ResourceFiles.Add(new CourseResourceFile
                    {
                        FileName = file.FileName,
                        FileUrl = $"/courseresources/{fileName}"
                    });
                }
            }

            _context.CourseResources.Add(res);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tải lên tài liệu thành công" });
        }

        [Authorize]
        [HttpGet("courses/{courseId}/resources")]
        public async Task<IActionResult> GetResources(int courseId)
        {
            var exists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!exists) return NotFound();

            var data = await _context.CourseResources
                .Include(r => r.ResourceFiles) // Load danh sách file
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new CourseResourceResponse
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    LinkUrl = r.LinkUrl,
                    CreatedAt = r.CreatedAt,
                    Files = r.ResourceFiles.Select(f => new FileDto
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        FileUrl = f.FileUrl
                    }).ToList()
                })
                .ToListAsync();

            return Ok(data);
        }

    }
}
