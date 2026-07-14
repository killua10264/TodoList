using TodoListBackend.DTOs.Category;

namespace TodoListBackend.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(int userId);
        Task<CategoryResponseDto?> GetCategoryByIdAsync(int id, int userId);
        Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto dto, int userId);
        Task DeleteCategoryAsync(int id, int userId);
        Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto, int userId);
    }
}
