using TodoListBackend.Models;
using TodoListBackend.DTOs.User;

namespace TodoListBackend.Services
{
    public interface IUserService
    {
        Task<User?> UpdateUserAsync(int id, UserUpdateDto dto);
    }
}