using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ProductManagementSystem.Helpers
{
    public static class ImageHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        private static readonly long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private const string ImagesFolder = "wwwroot/images/products";

        /// <summary>
        /// Saves a base64 image string to the local directory and returns the relative path
        /// </summary>
        /// <param name="base64Image">The base64 image string (with or without data URL prefix)</param>
        /// <param name="subfolder">Optional subfolder within the images directory</param>
        /// <returns>The relative path to the saved image</returns>
        public static async Task<string> SaveBase64ImageAsync(string base64Image, string subfolder = "")
        {
            if (string.IsNullOrEmpty(base64Image))
                throw new ArgumentException("No image data provided");

            // Remove data URL prefix if present (e.g., "data:image/jpeg;base64,")
            var base64Data = base64Image;
            var extension = ".jpg"; // default

            if (base64Image.StartsWith("data:image/"))
            {
                var parts = base64Image.Split(',');
                if (parts.Length == 2)
                {
                    var mimeType = parts[0].Split(';')[0].Replace("data:", "");
                    extension = GetExtensionFromMimeType(mimeType);
                    base64Data = parts[1];
                }
            }

            // Convert base64 to byte array
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid base64 image data");
            }

            // Validate file size
            if (imageBytes.Length > MaxFileSize)
                throw new ArgumentException($"Image size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");

            // Validate extension
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Image type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), ImagesFolder);
            if (!string.IsNullOrEmpty(subfolder))
                uploadPath = Path.Combine(uploadPath, subfolder);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            await File.WriteAllBytesAsync(filePath, imageBytes);

            // Return relative path for database storage
            var relativePath = Path.Combine("images", "products");
            if (!string.IsNullOrEmpty(subfolder))
                relativePath = Path.Combine(relativePath, subfolder);
            
            return Path.Combine(relativePath, fileName).Replace("\\", "/");
        }

        /// <summary>
        /// Saves an uploaded image to the local directory and returns the relative path
        /// </summary>
        /// <param name="file">The uploaded image file</param>
        /// <param name="subfolder">Optional subfolder within the images directory</param>
        /// <returns>The relative path to the saved image</returns>
        public static async Task<string> SaveImageAsync(IFormFile file, string subfolder = "")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            // Validate file size
            if (file.Length > MaxFileSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

            // Create directory if it doesn't exist
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), ImagesFolder);
            if (!string.IsNullOrEmpty(subfolder))
                uploadPath = Path.Combine(uploadPath, subfolder);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            var relativePath = Path.Combine("images", "products");
            if (!string.IsNullOrEmpty(subfolder))
                relativePath = Path.Combine(relativePath, subfolder);
            
            return Path.Combine(relativePath, fileName).Replace("\\", "/");
        }

        /// <summary>
        /// Gets the file extension from MIME type
        /// </summary>
        /// <param name="mimeType">The MIME type</param>
        /// <returns>The file extension</returns>
        private static string GetExtensionFromMimeType(string mimeType)
        {
            return mimeType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/bmp" => ".bmp",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        /// <summary>
        /// Deletes an image from the local directory
        /// </summary>
        /// <param name="relativePath">The relative path of the image to delete</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        public static bool DeleteImage(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return false;

            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine($"Error deleting image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the full URL for an image
        /// </summary>
        /// <param name="relativePath">The relative path stored in the database</param>
        /// <param name="baseUrl">The base URL of the application</param>
        /// <returns>The full URL to access the image</returns>
        public static string GetImageUrl(string relativePath, string baseUrl)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            return $"{baseUrl.TrimEnd('/')}/{relativePath}";
        }

        /// <summary>
        /// Validates if the uploaded file is a valid image
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > MaxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        /// <summary>
        /// Gets the content type for an image based on its extension
        /// </summary>
        /// <param name="fileName">The filename</param>
        /// <returns>The content type</returns>
        public static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
