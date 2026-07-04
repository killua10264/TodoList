using TodoListBackend.Models;
using TodoListBackend.DTOs.Category;

namespace TodoListBackend.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(int userId);
        Task<Category?> GetByIdAsync(int id, int userId);
        Task<Category?> GetByNameAsync(string name);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
        Task SaveChangesAsync();
    }
}