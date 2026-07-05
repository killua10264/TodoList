using TodoListBackend.DTOs.User;

namespace TodoListBackend.Services
{
    public interface IUserService
    {
        // FIX 4.2: Trả về non-nullable DTO vì Service sẽ throw exception nếu lỗi hoặc không tìm thấy user
        Task<UserResponseDto> UpdateUserAsync(int userId, UserUpdateDto dto);
    }
}