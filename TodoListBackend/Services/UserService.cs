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

        public async Task<UserResponseDto> UpdateProfileAsync(int userId, ProfileUpdateDto dto)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(userId, trackChanges: true);
            if (existingUser == null)
            {
                throw new NotFoundException("Tài khoản không tồn tại.");
            }
            var newUsername = dto.Username.Trim();
            if (!string.Equals(existingUser.Username, newUsername, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Users.ExistsByUsernameAsync(newUsername, userId))
                {
                    throw new BusinessException("Tên đăng nhập (Username) này đã được sử dụng bởi một tài khoản khác.");
                }
                existingUser.Username = newUsername;
            }

            existingUser.Bio = dto.Bio?.Trim();
            existingUser.Timezone = dto.Timezone.Trim();
            existingUser.Theme = dto.Theme.Trim().ToLowerInvariant();
            existingUser.Language = dto.Language.Trim().ToLowerInvariant();
            existingUser.FirstDayOfWeek = dto.FirstDayOfWeek.Trim();
            await _unitOfWork.SaveChangesAsync();

            return MapToResponseDto(existingUser);
        }

        public async Task<AvatarUpdateResult> UpdateAvatarAsync(
            int userId,
            string avatarUrl,
            string avatarPublicId)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(userId, trackChanges: true)
                ?? throw new NotFoundException("Tài khoản không tồn tại.");

            var previousPublicId = existingUser.AvatarPublicId;
            existingUser.AvatarUrl = avatarUrl;
            existingUser.AvatarPublicId = avatarPublicId;
            await _unitOfWork.SaveChangesAsync();

            return new AvatarUpdateResult(MapToResponseDto(existingUser), previousPublicId);
        }

        public async Task<AvatarUpdateResult> ClearAvatarAsync(int userId)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(userId, trackChanges: true)
                ?? throw new NotFoundException("Tài khoản không tồn tại.");

            var previousPublicId = existingUser.AvatarPublicId;
            existingUser.AvatarUrl = null;
            existingUser.AvatarPublicId = null;
            await _unitOfWork.SaveChangesAsync();

            return new AvatarUpdateResult(MapToResponseDto(existingUser), previousPublicId);
        }

        public async Task ChangePasswordAsync(int userId, ChangePassWordDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, trackChanges: true);
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

            // A password change invalidates every long-lived session, including
            // the transitional legacy token, so a stolen refresh token cannot
            // silently create a new access token after the password changed.
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            var activeSessions = await _unitOfWork.RefreshTokenSessions.GetActiveByUserIdAsync(userId);
            var revokedAt = DateTime.UtcNow;
            foreach (var session in activeSessions)
            {
                session.RevokedAt = revokedAt;
                session.RevocationReason = "password_changed";
                session.ConcurrencyToken = Guid.NewGuid();
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
