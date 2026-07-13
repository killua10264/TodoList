using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.User;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/users")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public UserController(IUserService userService, IPhotoService photoService)
        {
            _userService = userService;
            _photoService = photoService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int userId = GetCurrentUserId();
            var userProfile = await _userService.GetProfileAsync(userId);
            return Ok(userProfile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto dto)
        {
            int userId = GetCurrentUserId();
            var updatedUser = await _userService.UpdateUserAsync(userId, dto);
            return Ok(new { message = "Cập nhật thông tin tài khoản thành công.", data = updatedUser });
        }

        [HttpPost("profile/avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            int userId = GetCurrentUserId();

            // 1. Upload ảnh lên Cloudinary qua PhotoService
            var avatarUrl = await _photoService.UploadPhotoAsync(file);

            // 2. Cập nhật URL mới vào hồ sơ người dùng trong Database
            var updatedUser = await _userService.UpdateUserAsync(userId, new UserUpdateDto { AvatarUrl = avatarUrl });

            return Ok(new { message = "Tải ảnh đại diện lên Cloudinary thành công!", avatarUrl = avatarUrl, data = updatedUser });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassWordDto dto)
        {
            int userId = GetCurrentUserId();
            await _userService.ChangePasswordAsync(userId, dto);
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }
    }
}