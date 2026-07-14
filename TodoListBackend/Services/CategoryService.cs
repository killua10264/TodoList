using TodoListBackend.Models;
using TodoListBackend.DTOs.Category;
using TodoListBackend.Repositories;
using TodoListBackend.Mappings;
using TodoListBackend.Exceptions;

namespace TodoListBackend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(int userId)
        {
            var categories = await _unitOfWork.Categories.GetAllCategoriesAsync(userId);
            var categoryList = categories.ToList();

            if (!categoryList.Any())
            {
                var defaultCategories = new List<Category>
                {
                    new Category { Name = "Học tập", Color = "#4a5a3a", UserId = userId },
                    new Category { Name = "Công việc", Color = "#6b7a45", UserId = userId },
                    new Category { Name = "Khác", Color = "#8a8570", UserId = userId }
                };

                foreach (var cat in defaultCategories)
                {
                    await _unitOfWork.Categories.AddAsync(cat);
                }
                await _unitOfWork.SaveChangesAsync();

                categoryList = defaultCategories;
            }

            return categoryList.Select(p => p.ToResponseDto()!);
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id, int userId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, userId);
            return category?.ToResponseDto();
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto dto, int userId)
        {
            var category = new Category
            {
                Name = dto.Name,
                Color = dto.Color,
                UserId = userId
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category.ToResponseDto()
                ?? throw new BusinessException("Không thể tạo danh mục.");
        }

        public async Task DeleteCategoryAsync(int id, int userId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, userId);
            if (category == null)
                throw new NotFoundException($"Không tìm thấy danh mục có ID = {id}.");

            await _unitOfWork.Categories.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, CategoryUpdateDto dto, int userId)
        {
            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id, userId);
            if (existingCategory == null)
                throw new NotFoundException($"Không tìm thấy danh mục có ID = {id}.");

            existingCategory.Name = dto.Name;
            existingCategory.Color = dto.Color;

            await _unitOfWork.SaveChangesAsync();

            return existingCategory.ToResponseDto()!;
        }
    }
}
