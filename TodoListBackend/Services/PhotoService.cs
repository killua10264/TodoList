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
        public const long MaxAvatarBytes = 5 * 1024 * 1024;
        public const int MaxAvatarDimension = 8_000;

        private readonly Cloudinary _cloudinary;
        private readonly ILogger<PhotoService> _logger;

        public PhotoService(IOptions<CloudinarySettings> config, ILogger<PhotoService> logger)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
            _logger = logger;
        }

        public async Task<PhotoUploadResult> UploadPhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BusinessException("Vui lòng chọn file ảnh hợp lệ.");
            }

            if (file.Length > MaxAvatarBytes)
            {
                throw new BusinessException("Kích thước file ảnh không được vượt quá 5MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                throw new BusinessException("Định dạng ảnh không được hỗ trợ. Vui lòng chọn JPG, PNG hoặc WEBP.");
            }

            var expectedContentTypes = extension switch
            {
                ".jpg" or ".jpeg" => new[] { "image/jpeg" },
                ".png" => new[] { "image/png" },
                ".webp" => new[] { "image/webp" },
                _ => Array.Empty<string>()
            };

            if (!expectedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                throw new BusinessException("MIME type của ảnh không khớp với phần mở rộng.");
            }

            await ValidateImageSignatureAsync(file, extension);

            if (string.IsNullOrEmpty(_cloudinary.Api.Account.Cloud) || 
                string.IsNullOrEmpty(_cloudinary.Api.Account.ApiKey))
            {
                throw new BusinessException("Chưa cấu hình tài khoản Cloudinary trên Server (CloudName, ApiKey, ApiSecret).");
            }

            var uploadResult = new ImageUploadResult();
            var publicId = $"user_{Guid.NewGuid():N}";

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                    Folder = "todo_list_avatars",
                    PublicId = publicId
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }

            if (uploadResult.Error != null ||
                uploadResult.SecureUrl is null ||
                string.IsNullOrWhiteSpace(uploadResult.PublicId))
            {
                _logger.LogWarning("Cloudinary rejected avatar upload: {Message}", uploadResult.Error?.Message);
                throw new BusinessException("Không thể xử lý ảnh đại diện. Vui lòng thử lại bằng một file ảnh hợp lệ.");
            }

            if (uploadResult.Width <= 0 || uploadResult.Height <= 0 ||
                uploadResult.Width > MaxAvatarDimension || uploadResult.Height > MaxAvatarDimension)
            {
                await DeletePhotoAsync(uploadResult.PublicId);
                throw new BusinessException("Kích thước ảnh đại diện không được vượt quá 8000 x 8000 pixel.");
            }

            return new PhotoUploadResult(uploadResult.SecureUrl.AbsoluteUri, uploadResult.PublicId);
        }

        public async Task<bool> DeletePhotoAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return true;

            try
            {
                var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image,
                    Type = "upload",
                    Invalidate = true
                });

                if (result.Error != null)
                {
                    _logger.LogWarning("Cloudinary avatar deletion failed for {PublicId}: {Message}", publicId, result.Error.Message);
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Cloudinary avatar deletion threw for {PublicId}.", publicId);
                return false;
            }
        }

        private static async Task ValidateImageSignatureAsync(IFormFile file, string extension)
        {
            var header = new byte[12];
            await using var stream = file.OpenReadStream();
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length));

            var isValid = extension switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => bytesRead >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                ".webp" => bytesRead >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
                _ => false
            };

            if (!isValid)
            {
                throw new BusinessException("Nội dung file không phải ảnh hợp lệ.");
            }
        }
    }
}
