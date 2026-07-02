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

        public async Task<User?> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);

            if (existingUser == null)
            {
                return null;
            }

            existingUser.Username = dto.Username;
            existingUser.Email = dto.Email;

            await _userRepository.UpdateAsync(existingUser);
            await _userRepository.SaveChangesAsync();

            return existingUser;
        }
    }
}