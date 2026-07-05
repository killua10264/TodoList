using TodoListBackend.DTOs.Category;

namespace TodoListBackend.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(int userId);
        // FIX 4.2: Trả về non-nullable / void vì Service sẽ throw exception nếu lỗi
        Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto dto, int userId);
        Task DeleteCategoryAsync(int id, int userId);
        Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto, int userId);
    }
}