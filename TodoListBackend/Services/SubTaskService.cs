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

        // 1. HÀM BỔ SUNG: Phục vụ cho CreatedAtAction trong Controller
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
            // Tối ưu: Nếu có hàm ExistsAsync thì dùng, không thì tạm thời dùng GetById
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

            // Lưu ý rủi ro hiệu năng (Over-fetching) tại đây nếu lượng SubTask quá lớn.
            // Giải pháp triệt để: Viết hàm GetNextSortOrderAsync() dưới Repository để DB tự tính Max().
            var existingSubTasks = await _unitOfWork.SubTasks.GetByTodoIdAsync(dto.TodoId, userId);
            
            // Logic tính toán hình dáng lá cây rất sáng tạo!
            int nextSortOrder = existingSubTasks.Any() ? existingSubTasks.Max(s => s.SortOrder) + 1 : 1;
            int nextLeafShape = existingSubTasks.Count() % 5; 

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

            // Vì update nên hàm này bắt buộc Entity phải được Tracking bởi EF Core
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