using TodoListBackend.DTOs.User;

namespace TodoListBackend.Services
{
    public interface IUserService
    {
        Task<UserResponseDto> GetProfileAsync(int userId);
        Task<UserResponseDto> UpdateProfileAsync(int userId, ProfileUpdateDto dto);
        Task<AvatarUpdateResult> UpdateAvatarAsync(int userId, string avatarUrl, string avatarPublicId);
        Task<AvatarUpdateResult> ClearAvatarAsync(int userId);
        Task ChangePasswordAsync(int userId, ChangePassWordDto dto);
    }
}
