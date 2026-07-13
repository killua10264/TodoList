using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TodoListBackend.Exceptions;
using TodoListBackend.Models;

namespace TodoListBackend.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadPhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BusinessException("Vui lòng chọn file ảnh hợp lệ.");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                throw new BusinessException("Kích thước file ảnh không được vượt quá 5MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new BusinessException("Định dạng ảnh không được hỗ trợ. Vui lòng chọn JPG, PNG hoặc WEBP.");
            }

            if (string.IsNullOrEmpty(_cloudinary.Api.Account.Cloud) || 
                string.IsNullOrEmpty(_cloudinary.Api.Account.ApiKey))
            {
                throw new BusinessException("Chưa cấu hình tài khoản Cloudinary trên Server (CloudName, ApiKey, ApiSecret).");
            }

            var uploadResult = new ImageUploadResult();

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                    Folder = "todo_list_avatars"
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null)
            {
                throw new BusinessException($"Lỗi tải ảnh lên Cloudinary: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.AbsoluteUri;
        }
    }
}
