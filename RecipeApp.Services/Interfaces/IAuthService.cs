using RecipeApp.Common.DTOs;

namespace RecipeApp.Services.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(UserAdminDto user);
    }
}