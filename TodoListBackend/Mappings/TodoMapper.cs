using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Mappings
{
    public static class TodoMapper
    {
        public static TodoResponseDto? ToResponseDto(this Todo todoModel)
        {
            if (todoModel == null) return null;
            
            return new TodoResponseDto
            {
                Id = todoModel.Id,
                Title = todoModel.Title,
                Description = todoModel.Description,
                IsCompleted = todoModel.IsCompleted,
                Priority = todoModel.Priority,
                DueDate = todoModel.DueDate,
                CategoryId = todoModel.CategoryId,
                CategoryName = todoModel.Category?.Name ?? string.Empty,
                CategoryColor = todoModel.Category?.Color ?? string.Empty
            };
        }
    }
}