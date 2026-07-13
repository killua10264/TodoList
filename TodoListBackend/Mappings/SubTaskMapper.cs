using TodoListBackend.Models;
using TodoListBackend.DTOs.SubTask;

namespace TodoListBackend.Mappings
{
    public static class SubTaskMapper
    {
        public static SubTaskResponseDto? ToResponseDto(this SubTask? subTaskModel)
        {
            if (subTaskModel == null) return null;

            return new SubTaskResponseDto
            {
                Id = subTaskModel.Id,
                Title = subTaskModel.Title,
                IsCompleted = subTaskModel.IsCompleted,
                SortOrder = subTaskModel.SortOrder,
                LeafShape = subTaskModel.LeafShape,
                CreatedAt = subTaskModel.CreatedAt,
                TodoId = subTaskModel.TodoId
            };
        }
    }
}
