using Microsoft.AspNetCore.Http;

namespace Engli3m.Infrastructure.Helper
{
    public static class FileHelper
    {
        public static async Task<string> SaveImageAsync(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0) return null!;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folderPath, fileName);

            Directory.CreateDirectory(folderPath);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }
        public static string GetExtension(IFormFile file)
        {
            return (Path.GetExtension(file.FileName));
        }
    }
}