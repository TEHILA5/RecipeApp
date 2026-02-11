using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RecipeApp.Service.Interfaces;

namespace RecipeApp.Service.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _imageFolder = "uploads/recipes";

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
            int x=5;
        }

        /// <summary>
        /// שמירת תמונה בשרת והחזרת שם הקובץ
        /// </summary>
        public async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("No image file provided");

            // בדיקת סוג קובץ
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only JPG, PNG, GIF, and WEBP are allowed.");

            // בדיקת גודל (מקסימום 5MB)
            if (imageFile.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size cannot exceed 5MB");

            // יצירת שם קובץ ייחודי
            var fileName = $"{Guid.NewGuid()}{extension}";

            // יצירת תיקייה אם לא קיימת
            var uploadPath = Path.Combine(_environment.WebRootPath, _imageFolder);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // שמירת הקובץ
            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fileName;
        }

        /// <summary>
        /// מחיקת תמונה מהשרת
        /// </summary>
        public async Task<bool> DeleteImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var filePath = Path.Combine(_environment.WebRootPath, _imageFolder, fileName);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                return true;
            }

            return false;
        }

        /// <summary>
        /// קבלת URL מלא לתמונה
        /// </summary>
        public string GetImageUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            return $"/{_imageFolder}/{fileName}";
        }
    }
}