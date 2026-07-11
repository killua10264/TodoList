using TodoListBackend.Models;
using TodoListBackend.DTOs;
using TodoListBackend.DTOs.Todo;
using TodoListBackend.Repositories;
using TodoListBackend.Mappings;
using TodoListBackend.Exceptions;

namespace TodoListBackend.Services
{
    public class TodoService : ITodoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TodoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponse<TodoResponseDto>> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20)
        {
            var (todos, totalCount) = await _unitOfWork.Todos.GetAllTodosAsync(userId, page, pageSize);
            return new PaginatedResponse<TodoResponseDto>
            {
                Items = todos.Select(t => t.ToResponseDto()!),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<TodoResponseDto?> GetTodoByIdAsync(int id, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId);
            if (todo == null) return null;
            return todo.ToResponseDto();
        }

        public async Task<TodoResponseDto> CreateTodoAsync(TodoCreateDto dto, int userId)
        {
            if (dto.DueDate < DateTime.UtcNow.Date)
            {
                throw new BusinessException("Ngày hết hạn không được nằm trong quá khứ.");
            }

            // Kiểm tra xem CategoryId có tồn tại trong CSDL cho user này không
            var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, userId);
            if (category == null)
            {
                // Nếu ID hardcode (1, 2, 3) không trùng với ID thật trong DB, tìm theo tên hoặc lấy danh mục đầu tiên
                var userCategories = (await _unitOfWork.Categories.GetAllCategoriesAsync(userId)).ToList();
                string targetName = dto.CategoryId == 1 ? "Học tập" : (dto.CategoryId == 2 ? "Công việc" : "Khác");
                var matched = userCategories.FirstOrDefault(c => c.Name == targetName) ?? userCategories.FirstOrDefault();

                if (matched != null)
                {
                    dto.CategoryId = matched.Id;
                }
                else
                {
                    var newCat = new Category { Name = targetName, Color = "#4a5a3a", UserId = userId };
                    await _unitOfWork.Categories.AddAsync(newCat);
                    await _unitOfWork.SaveChangesAsync();
                    dto.CategoryId = newCat.Id;
                }
            }

            var newTodo = new Todo
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId,
                CategoryId = dto.CategoryId,
                IsCompleted = false,
                IsDeleted = false
            };

            await _unitOfWork.Todos.AddAsync(newTodo);
            await _unitOfWork.SaveChangesAsync();

            var createdTodo = await _unitOfWork.Todos.GetByIdAsync(newTodo.Id, userId);
            return createdTodo?.ToResponseDto()
                ?? throw new BusinessException("Không thể tạo công việc.");
        }

        // FIX 4.2: Throw NotFoundException thay vì return false
        public async Task DeleteTodoAsync(int id, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId);
            if (todo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            todo.IsDeleted = true;
            todo.UpdatedAt = DateTime.UtcNow;

            // FIX 2.4: Không cần gọi UpdateAsync — Change Tracker tự detect
            await _unitOfWork.SaveChangesAsync();
        }

        // FIX 4.2: Throw NotFoundException thay vì return null
        public async Task<TodoResponseDto> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId)
        {
            var existingTodo = await _unitOfWork.Todos.GetByIdAsync(id, userId);
            if (existingTodo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            if (dto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId.Value, userId);
                if (category == null)
                {
                    var userCategories = (await _unitOfWork.Categories.GetAllCategoriesAsync(userId)).ToList();
                    string targetName = dto.CategoryId.Value == 1 ? "Học tập" : (dto.CategoryId.Value == 2 ? "Công việc" : "Khác");
                    var matched = userCategories.FirstOrDefault(c => c.Name == targetName) ?? userCategories.FirstOrDefault();

                    if (matched != null)
                    {
                        dto.CategoryId = matched.Id;
                    }
                    else
                    {
                        var newCat = new Category { Name = targetName, Color = "#4a5a3a", UserId = userId };
                        await _unitOfWork.Categories.AddAsync(newCat);
                        await _unitOfWork.SaveChangesAsync();
                        dto.CategoryId = newCat.Id;
                    }
                }
            }

            existingTodo.Title = dto.Title ?? existingTodo.Title;
            existingTodo.Description = dto.Description ?? existingTodo.Description;
            existingTodo.Priority = dto.Priority ?? existingTodo.Priority;
            existingTodo.DueDate = dto.DueDate ?? existingTodo.DueDate;
            existingTodo.IsCompleted = dto.IsCompleted ?? existingTodo.IsCompleted;
            existingTodo.CategoryId = dto.CategoryId ?? existingTodo.CategoryId;
            existingTodo.UpdatedAt = DateTime.UtcNow;

            // FIX 2.4: Không cần gọi UpdateAsync — Change Tracker tự detect
            await _unitOfWork.SaveChangesAsync();

            return existingTodo.ToResponseDto()!;
        }
    }
}