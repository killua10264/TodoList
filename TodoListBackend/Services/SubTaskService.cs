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
        public async Task<SubTaskResponseDto> GetSubTaskByIdAsync(int id, int userId)
        {
            var subTask = await _unitOfWork.SubTasks.GetByIdAsync(id, userId, trackChanges: false);
            if (subTask == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc con với ID {id}");
            }
            return subTask.ToResponseDto()!;
        }

        public async Task<IEnumerable<SubTaskResponseDto>> GetSubTasksByTodoIdAsync(int todoId, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(todoId, userId, trackChanges: false);
            if (todo == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc với ID {todoId}");
            }

            var subTasks = await _unitOfWork.SubTasks.GetByTodoIdAsync(todoId, userId);
            return subTasks.Select(s => s.ToResponseDto()!);
        }

        public async Task<SubTaskResponseDto> CreateSubTaskAsync(SubTaskCreateDto dto, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(dto.TodoId, userId, trackChanges: false);
            if (todo == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc với ID {dto.TodoId}");
            }
            int currentCount = await _unitOfWork.SubTasks.GetCountByTodoIdAsync(dto.TodoId);
            int maxSortOrder = await _unitOfWork.SubTasks.GetMaxSortOrderByTodoIdAsync(dto.TodoId);
            
            int nextSortOrder = currentCount > 0 ? maxSortOrder + 1 : 1;
            int nextLeafShape = currentCount % 5;

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
            var subTask = await _unitOfWork.SubTasks.GetByIdAsync(id, userId, trackChanges: true);
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
            var subTask = await _unitOfWork.SubTasks.GetByIdAsync(id, userId, trackChanges: true);
            if (subTask == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc con với ID {id}");
            }

            await _unitOfWork.SubTasks.DeleteAsync(subTask);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
