using Microsoft.EntityFrameworkCore;
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
            await SaveWithConcurrencyHandlingAsync();

            return subTask.ToResponseDto()!;
        }

        public async Task<SubTaskResponseDto> UpdateSubTaskAsync(int id, SubTaskUpdateDto dto, int userId)
        {
            var subTask = await _unitOfWork.SubTasks.GetByIdAsync(id, userId, trackChanges: true);
            if (subTask == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc con với ID {id}");
            }

            EnsureVersion(subTask.Version, dto.Version);

            subTask.Title = dto.Title;
            subTask.IsCompleted = dto.IsCompleted;
            await SaveWithConcurrencyHandlingAsync();

            return subTask.ToResponseDto()!;
        }

        public async Task DeleteSubTaskAsync(int id, int userId, uint expectedVersion)
        {
            var subTask = await _unitOfWork.SubTasks.GetByIdAsync(id, userId, trackChanges: true);
            if (subTask == null)
            {
                throw new NotFoundException($"Không tìm thấy công việc con với ID {id}");
            }

            EnsureVersion(subTask.Version, expectedVersion);

            await _unitOfWork.SubTasks.DeleteAsync(subTask);
            await SaveWithConcurrencyHandlingAsync();
        }

        public async Task<IEnumerable<SubTaskResponseDto>> ReorderSubTasksAsync(
            int todoId,
            SubTaskOrderRequestDto request,
            int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(todoId, userId, trackChanges: false);
            if (todo is null)
            {
                throw new NotFoundException($"Không tìm thấy công việc với ID {todoId}");
            }

            var subTasks = (await _unitOfWork.SubTasks.GetByTodoIdAsync(todoId, userId, trackChanges: true)).ToList();
            if (request.Items.Count != subTasks.Count)
            {
                throw new BusinessException(
                    "Danh sách sắp xếp phải chứa đúng toàn bộ công việc con của công việc cha.",
                    StatusCodes.Status400BadRequest,
                    "invalid_subtask_order");
            }

            var requestedIds = request.Items.Select(item => item.SubTaskId).ToHashSet();
            var actualIds = subTasks.Select(item => item.Id).ToHashSet();
            var requestedOrders = request.Items.Select(item => item.SortOrder).ToHashSet();
            var expectedOrders = Enumerable.Range(1, subTasks.Count).ToHashSet();

            if (!requestedIds.SetEquals(actualIds) || !requestedOrders.SetEquals(expectedOrders))
            {
                throw new BusinessException(
                    "Danh sách sắp xếp phải chứa đúng ID và thứ tự liên tục của các công việc con.",
                    StatusCodes.Status400BadRequest,
                    "invalid_subtask_order");
            }

            var orderById = request.Items.ToDictionary(item => item.SubTaskId, item => item.SortOrder);
            foreach (var subTask in subTasks)
            {
                subTask.SortOrder = orderById[subTask.Id];
            }

            await SaveWithConcurrencyHandlingAsync();
            return subTasks.OrderBy(item => item.SortOrder).Select(item => item.ToResponseDto()!);
        }

        private static void EnsureVersion(uint currentVersion, uint? expectedVersion)
        {
            if (expectedVersion.HasValue && expectedVersion.Value != currentVersion)
            {
                throw new BusinessException(
                    "Công việc con đã được thay đổi bởi một phiên khác. Vui lòng tải lại dữ liệu.",
                    StatusCodes.Status409Conflict,
                    "concurrency_conflict");
            }
        }

        private async Task SaveWithConcurrencyHandlingAsync()
        {
            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BusinessException(
                    "Công việc con đã được thay đổi bởi một phiên khác. Vui lòng tải lại dữ liệu.",
                    StatusCodes.Status409Conflict,
                    "concurrency_conflict");
            }
        }
    }
}
