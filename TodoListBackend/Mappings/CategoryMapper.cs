using TodoListBackend.Models;
using TodoListBackend.DTOs.Category;

namespace TodoListBackend.Mappings
{
    public static class CategoryMapper
    {
        public static CategoryResponseDto? ToResponseDto(this Category? categoryModel)
        {
            if (categoryModel == null) return null;

            return new CategoryResponseDto
            {
                Id = categoryModel.Id,
                Name = categoryModel.Name,
                Color = categoryModel.Color,
                TodoCount = categoryModel.Todos?.Count(t => !t.IsDeleted) ?? 0,
                CompletedTodoCount = categoryModel.Todos?.Count(t => t.IsCompleted && !t.IsDeleted) ?? 0
            };
        }
    }
}
