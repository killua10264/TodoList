using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.User;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/users")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            var updatedUser = await _userService.UpdateUserAsync(userId, dto);

            if (updatedUser == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại." });
            }

            return Ok(new { message = "Cập nhật thông tin tài khoản thành công.", data = updatedUser });
        }
    }
}