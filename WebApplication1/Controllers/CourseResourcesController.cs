using ClassMate.Api.Data;
using ClassMate.Api.DTOs;
using ClassMate.Api.Entities;
using ClassMate.Api.Utils;
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
        public async Task<IActionResult> UploadResource(
            int courseId,
            [FromForm] CourseResourceCreateRequest request)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            string? fileUrl = null;
            if (request.File != null)
            {
                var folder = Path.Combine(_env.ContentRootPath, "CourseResources");
                Directory.CreateDirectory(folder);

                var ext = Path.GetExtension(request.File.FileName);
                var fileName = $"cr_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await request.File.CopyToAsync(stream);

                fileUrl = $"/courseresources/{fileName}";
            }

            var res = new CourseResource
            {
                CourseId = courseId,
                Title = request.Title,
                Description = request.Description,
                FileUrl = fileUrl,
                LinkUrl = request.LinkUrl
            };

            _context.CourseResources.Add(res);
            await _context.SaveChangesAsync();

            return Ok(new CourseResourceResponse
            {
                Id = res.Id,
                Title = res.Title,
                Description = res.Description,
                FileUrl = res.FileUrl,
                LinkUrl = res.LinkUrl,
                CreatedAt = res.CreatedAt.ToVietnamTime()
            });
        }

        [Authorize]
        [HttpGet("courses/{courseId}/resources")]
        public async Task<IActionResult> GetResources(int courseId)
        {
            var exists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!exists) return NotFound();

            var data = await _context.CourseResources
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(data.Select(r => new CourseResourceResponse
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                FileUrl = r.FileUrl,
                LinkUrl = r.LinkUrl,
                CreatedAt = r.CreatedAt.ToVietnamTime()
            }));
        }
    }
}
