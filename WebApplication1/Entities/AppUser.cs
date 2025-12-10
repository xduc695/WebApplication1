using Microsoft.AspNetCore.Identity;

namespace ClassMate.Api.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
