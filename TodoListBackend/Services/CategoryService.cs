using TodoListBackend.Models;
using TodoListBackend.DTOs.Category;
using TodoListBackend.Repositories;

namespace TodoListBackend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> CreateCategoryAsync(CategoryCreateDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Color = dto.Color
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            await _categoryRepository.DeleteAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Category?> UpdateCategoryAsync(int id, CategoryUpdateDto dto)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id);
            if (existingCategory == null) return null;

            existingCategory.Name = dto.Name;
            existingCategory.Color = dto.Color;

            await _categoryRepository.UpdateAsync(existingCategory);
            await _categoryRepository.SaveChangesAsync();

            return existingCategory;
        }
    }
}