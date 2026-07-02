using TodoListBackend.Models;
using TodoListBackend.DTOs.Category;

namespace TodoListBackend.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> CreateCategoryAsync(CategoryCreateDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<Category?> UpdateCategoryAsync(int id, CategoryUpdateDto dto);
    }
}