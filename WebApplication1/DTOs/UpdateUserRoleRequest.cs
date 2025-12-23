using System.ComponentModel.DataAnnotations;

namespace ClassMate.Api.DTOs
{
    public class UpdateUserRoleRequest
    {
        [Required]
        public string Role { get; set; } = null!; // "Student" / "Teacher" / "Admin"
    }
}
