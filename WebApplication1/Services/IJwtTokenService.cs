using ClassMate.Api.Entities;

namespace ClassMate.Api.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(AppUser user, string role);
    }
}
