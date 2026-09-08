using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using TodoListBackend.DTOs.User;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/users")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IPhotoService photoService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _photoService = photoService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int userId = GetCurrentUserId();
            var userProfile = await _userService.GetProfileAsync(userId);
            return Ok(userProfile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            int userId = GetCurrentUserId();
            var updatedUser = await _userService.UpdateProfileAsync(userId, dto);
            return Ok(new { message = "Cập nhật thông tin tài khoản thành công.", data = updatedUser });
        }

        [HttpPost("profile/avatar")]
        [RequestSizeLimit(PhotoService.MaxAvatarBytes + (64 * 1024))]
        [EnableRateLimiting("AvatarLimit")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            int userId = GetCurrentUserId();
            var upload = await _photoService.UploadPhotoAsync(file);
            AvatarUpdateResult updated;

            try
            {
                updated = await _userService.UpdateAvatarAsync(userId, upload.Url, upload.PublicId);
            }
            catch
            {
                await _photoService.DeletePhotoAsync(upload.PublicId);
                throw;
            }

            if (!string.IsNullOrWhiteSpace(updated.PreviousPublicId) &&
                !string.Equals(updated.PreviousPublicId, upload.PublicId, StringComparison.Ordinal))
            {
                _ = DeleteOldAvatarBestEffortAsync(updated.PreviousPublicId);
            }

            return Ok(new { message = "Tải ảnh đại diện thành công!", avatarUrl = upload.Url, data = updated.Profile });
        }

        [HttpDelete("profile/avatar")]
        public async Task<IActionResult> DeleteAvatar()
        {
            var result = await _userService.ClearAvatarAsync(GetCurrentUserId());
            if (!string.IsNullOrWhiteSpace(result.PreviousPublicId))
            {
                _ = DeleteOldAvatarBestEffortAsync(result.PreviousPublicId);
            }

            return Ok(new { message = "Đã chuyển về ảnh đại diện mặc định.", data = result.Profile });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassWordDto dto)
        {
            int userId = GetCurrentUserId();
            await _userService.ChangePasswordAsync(userId, dto);
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        private async Task DeleteOldAvatarBestEffortAsync(string publicId)
        {
            try
            {
                await _photoService.DeletePhotoAsync(publicId);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to delete old avatar asset {PublicId}.", publicId);
            }
        }
    }
}
