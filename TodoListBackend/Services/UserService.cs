using TodoListBackend.Models;
using TodoListBackend.DTOs.User;
using TodoListBackend.Repositories;

namespace TodoListBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int userId, UserUpdateDto dto)
        {
            var existingUser = await _userRepository.GetByIdAsync(userId);

            if (existingUser == null)
            {
                return null;
            }

            existingUser.Username = dto.Username?.Trim() ?? existingUser.Username;
            existingUser.Email = dto.Email?.Trim() ?? existingUser.Email;

            await _userRepository.UpdateAsync(existingUser);
            await _userRepository.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = existingUser.Id,
                Username = existingUser.Username,
                Email = existingUser.Email
            };
        }
    }
}