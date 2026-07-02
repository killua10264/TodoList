using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.User;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto dto)
        {
            //Tempt
            int userId = 1; // Replace with actual user ID from authentication context

            var updatedUser = await _userService.UpdateUserAsync(userId, dto);

            if (updatedUser == null)
            {
                return NotFound(new { message = "Tài khoản không tồn tại." });
            }

            return Ok(new { message = "Cập nhật thông tin tài khoản thành công."});
        }
    }
}