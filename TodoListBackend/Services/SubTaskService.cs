using TodoListBackend.Models;
using TodoListBackend.DTOs.SubTask;
using TodoListBackend.Repositories;
using TodoListBackend.Mappings;
using TodoListBackend.Exceptions;

namespace TodoListBackend.Services
{
    public class SubTaskService : ISubTaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubTaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SubTaskResponseDto>> GetSubTasksByTodoIdAsync(int todoId, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(todoId, userId);
            if (todo == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc với ID {todoId}");
            }

            var subTasks = await _unitOfWork.SubTasks.GetByTodoIdAsync(todoId, userId);
            return subTasks.Select(s => s.ToResponseDto()!);
        }

        public async Task<SubTaskResponseDto> CreateSubTaskAsync(SubTaskCreateDto dto, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(dto.TodoId, userId);
            if (todo == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc với ID {dto.TodoId}");
            }

            var existingSubTasks = await _unitOfWork.SubTasks.GetByTodoIdAsync(dto.TodoId, userId);
            int nextSortOrder = existingSubTasks.Any() ? existingSubTasks.Max(s => s.SortOrder) + 1 : 1;
            int nextLeafShape = existingSubTasks.Count() % 5; // 5 hình lá khác nhau từ 0 đến 4

            var subTask = new SubTask
            {
                Title = dto.Title,
                IsCompleted = false,
                SortOrder = nextSortOrder,
                LeafShape = nextLeafShape,
                CreatedAt = DateTime.UtcNow,
                TodoId = dto.TodoId
            };

            await _unitOfWork.SubTasks.AddAsync(subTask);
            await _unitOfWork.SaveChangesAsync();

            return subTask.ToResponseDto()!;
        }

        public async Task<SubTaskResponseDto> UpdateSubTaskAsync(int id, SubTaskUpdateDto dto, int userId)
        {
            var subTask = await _unitOfWork.SubTasks.GetByIdAsync(id, userId);
            if (subTask == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc con với ID {id}");
            }

            subTask.Title = dto.Title;
            subTask.IsCompleted = dto.IsCompleted;
            subTask.SortOrder = dto.SortOrder;

            await _unitOfWork.SaveChangesAsync();

            return subTask.ToResponseDto()!;
        }

        public async Task DeleteSubTaskAsync(int id, int userId)
        {
            var subTask = await _unitOfWork.SubTasks.GetByIdAsync(id, userId);
            if (subTask == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc con với ID {id}");
            }

            await _unitOfWork.SubTasks.DeleteAsync(subTask);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
