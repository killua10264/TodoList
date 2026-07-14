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

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                DisplayName = user.DisplayName,
                Bio = user.Bio,
                Timezone = user.Timezone ?? "Asia/Ho_Chi_Minh",
                Theme = user.Theme ?? "light",
                Language = user.Language ?? "vi",
                FirstDayOfWeek = user.FirstDayOfWeek ?? "Monday",
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDto> GetProfileAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Tài khoản không tồn tại.");
            }
            return MapToResponseDto(user);
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
                var newUsername = dto.Username.Trim();
                if (!string.Equals(existingUser.Username, newUsername, StringComparison.OrdinalIgnoreCase))
                {
                    if (await _unitOfWork.Users.ExistsByUsernameAsync(newUsername, userId))
                    {
                        throw new BusinessException("Tên đăng nhập (Username) này đã được sử dụng bởi một tài khoản khác.");
                    }
                    existingUser.Username = newUsername;
                }
            }

            if (dto.AvatarUrl != null)
            {
                existingUser.AvatarUrl = dto.AvatarUrl;
            }
            if (dto.DisplayName != null)
            {
                existingUser.DisplayName = dto.DisplayName.Trim();
            }
            if (dto.Bio != null)
            {
                existingUser.Bio = dto.Bio.Trim();
            }
            if (!string.IsNullOrWhiteSpace(dto.Timezone))
            {
                existingUser.Timezone = dto.Timezone.Trim();
            }
            if (!string.IsNullOrWhiteSpace(dto.Theme))
            {
                existingUser.Theme = dto.Theme.Trim();
            }
            if (!string.IsNullOrWhiteSpace(dto.Language))
            {
                existingUser.Language = dto.Language.Trim();
            }
            if (!string.IsNullOrWhiteSpace(dto.FirstDayOfWeek))
            {
                existingUser.FirstDayOfWeek = dto.FirstDayOfWeek.Trim();
            }

            // FIX 2.4: Không cần gọi UpdateAsync — Change Tracker tự detect changes
            await _unitOfWork.SaveChangesAsync();

            return MapToResponseDto(existingUser);
        }

        public async Task ChangePasswordAsync(int userId, ChangePassWordDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Tài khoản không tồn tại.");
            }

            if (string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
            {
                throw new BusinessException("Mật khẩu hiện tại (mật khẩu cũ) không chính xác. Vui lòng kiểm tra lại.");
            }

            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.Password))
            {
                throw new BusinessException("Mật khẩu mới không được trùng với mật khẩu hiện tại.");
            }

            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                throw new BusinessException("Mật khẩu xác nhận không khớp với mật khẩu mới.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}