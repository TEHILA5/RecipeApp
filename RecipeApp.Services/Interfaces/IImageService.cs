using Microsoft.AspNetCore.Http;

namespace RecipeApp.Service.Interfaces
{
    public interface IImageService
    {
        Task<string> SaveImage(IFormFile imageFile);
        Task<bool> DeleteImage(string fileName);
        string GetImageUrl(string fileName);
    }
}
