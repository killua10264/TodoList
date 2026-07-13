using TodoListBackend.DTOs.User;

namespace TodoListBackend.Services
{
    public interface IUserService
    {
        Task<UserResponseDto> GetProfileAsync(int userId);
        Task<UserResponseDto> UpdateUserAsync(int userId, UserUpdateDto dto);
        Task ChangePasswordAsync(int userId, ChangePassWordDto dto);
    }
}