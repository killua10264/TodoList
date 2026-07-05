using TodoListBackend.Models;
using TodoListBackend.DTOs.User;
using TodoListBackend.Repositories;
using TodoListBackend.Exceptions;

namespace TodoListBackend.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserResponseDto> UpdateUserAsync(int userId, UserUpdateDto dto)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(userId);
            if (existingUser == null)
            {
                throw new NotFoundException("Tài khoản không tồn tại.");
            }

            // FIX 3.7: Check email uniqueness (ngoại trừ user hiện tại)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim();
                if (!string.Equals(existingUser.Email, newEmail, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _unitOfWork.Users.ExistsByEmailAsync(newEmail, userId))
                    {
                        throw new BusinessException("Email này đã được sử dụng bởi một tài khoản khác.");
                    }
                    existingUser.Email = newEmail;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                existingUser.Username = dto.Username.Trim();
            }

            // FIX 2.4: Không cần gọi UpdateAsync — Change Tracker tự detect changes
            await _unitOfWork.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = existingUser.Id,
                Username = existingUser.Username,
                Email = existingUser.Email
            };
        }
    }
}