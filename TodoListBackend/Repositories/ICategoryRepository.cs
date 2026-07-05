using TodoListBackend.Models;

namespace TodoListBackend.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync(int userId);
        Task<Category?> GetByIdAsync(int id, int userId);
        Task<Category?> GetByNameAsync(string name);
        Task AddAsync(Category category);
        Task DeleteAsync(Category category);
        // FIX 2.4: Xóa UpdateAsync — EF Change Tracker tự detect changes
    }
}