using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ClassMate.Api.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

   

        // File avatar (có thể null)
        public IFormFile? Avatar { get; set; }
    }
}
