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
                ProjectId = todoModel.ProjectId,
                ProjectName = todoModel.Project?.Name ?? string.Empty,
                ProjectColor = todoModel.Project?.Color ?? string.Empty
            };
        }
    }
}