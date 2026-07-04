using TodoListBackend.Models;
using TodoListBackend.DTOs.Category;
using TodoListBackend.Repositories;
using TodoListBackend.Mappings;

namespace TodoListBackend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(int userId)
        {
            return await _categoryRepository.GetAllCategoriesAsync(userId);
        }

        public async Task<CategoryResponseDto?> CreateCategoryAsync(CategoryCreateDto dto, int userId)
        {
            var category = new Category
            {
                Name = dto.Name,
                Color = dto.Color,
                UserId = userId
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return category.ToResponseDto();
        }

        public async Task<bool> DeleteCategoryAsync(int id, int userId)
        {
            var category = await _categoryRepository.GetByIdAsync(id, userId);
            if (category == null) return false;

            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return true;
        }

        public async Task<CategoryResponseDto?> UpdateCategoryAsync(int id, CategoryUpdateDto dto, int userId)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id, userId);
            if (existingCategory == null) return null;

            existingCategory.Name = dto.Name;
            existingCategory.Color = dto.Color;

            await _categoryRepository.UpdateAsync(existingCategory);
            await _categoryRepository.SaveChangesAsync();

            return existingCategory.ToResponseDto();
        }
    }
}