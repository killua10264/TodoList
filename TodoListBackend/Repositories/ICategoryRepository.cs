using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync(int userId);
        Task<Category?> GetByIdAsync(int id, int userId);
        Task<Category?> GetByNameAsync(string name, int userId);
        Task<bool> ExistsByNameAsync(string name, int userId, int? excludeId = null);
        Task AddAsync(Category category);
        Task DeleteAsync(Category category);
    }
}
