using TodoListBackend.DTOs.User;

namespace TodoListBackend.Services;

public sealed record AvatarUpdateResult(UserResponseDto Profile, string? PreviousPublicId);
