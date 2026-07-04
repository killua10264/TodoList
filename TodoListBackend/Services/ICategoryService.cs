using TodoListBackend.DTOs.Category;

namespace TodoListBackend.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(int userId);
        Task<CategoryResponseDto?> CreateCategoryAsync(CategoryCreateDto dto, int userId);
        Task<bool> DeleteCategoryAsync(int id, int userId);
        Task<CategoryResponseDto?> UpdateCategoryAsync(int id, CategoryUpdateDto dto, int userId);
    }
}