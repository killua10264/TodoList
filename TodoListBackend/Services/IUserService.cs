using TodoListBackend.DTOs.User;

namespace TodoListBackend.Services
{
    public interface IUserService
    {
        Task<UserResponseDto?> UpdateUserAsync(int userId, UserUpdateDto dto);
    }
}