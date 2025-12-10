using ClassMate.Api.Data;
using ClassMate.Api.DTOs;
using ClassMate.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassMate.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // Ai cũng xem được
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAll()
        {
            var courses = await _context.Courses
                .Select(c => new CourseResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CourseResponse>> GetById(int id)
        {
            var c = await _context.Courses.FindAsync(id);
            if (c == null) return NotFound();

            return Ok(new CourseResponse
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Description = c.Description
            });
        }

        // Chỉ Teacher/Admin được tạo
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var exist = await _context.Courses.AnyAsync(x => x.Code == request.Code);
            if (exist) return BadRequest(new { message = "Course code already exists" });

            var course = new Course
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = course.Id }, new CourseResponse
            {
                Id = course.Id,
                Name = course.Name,
                Code = course.Code,
                Description = course.Description
            });
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CourseCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var c = await _context.Courses.FindAsync(id);
            if (c == null) return NotFound();

            var exist = await _context.Courses
                .AnyAsync(x => x.Code == request.Code && x.Id != id);
            if (exist) return BadRequest(new { message = "Course code already exists" });

            c.Name = request.Name;
            c.Code = request.Code;
            c.Description = request.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.Courses.FindAsync(id);
            if (c == null) return NotFound();

            _context.Courses.Remove(c);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
