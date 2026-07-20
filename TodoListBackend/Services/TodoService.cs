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

        public async Task<PaginatedResponse<TodoResponseDto>> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20, string? filter = null, int? categoryId = null, string? status = null, string? sortBy = null, bool? isHidden = false)
        {
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                categoryId = await ResolveCategoryIdAsync(categoryId.Value, userId);
            }

            var (todos, totalCount) = await _unitOfWork.Todos.GetAllTodosAsync(userId, page, pageSize, filter, categoryId, status, sortBy, isHidden);
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

            dto.CategoryId = await ResolveCategoryIdAsync(dto.CategoryId, userId);

            var newTodo = new Todo
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = DateTime.SpecifyKind(dto.DueDate, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId,
                CategoryId = dto.CategoryId,
                IsCompleted = false,
                IsHidden = false,
                IsDeleted = false
            };

            await _unitOfWork.Todos.AddAsync(newTodo);
            await _unitOfWork.SaveChangesAsync();

            var createdTodo = await _unitOfWork.Todos.GetByIdAsync(newTodo.Id, userId);
            return createdTodo?.ToResponseDto()
                ?? throw new BusinessException("Không thể tạo công việc.");
        }

        public async Task DeleteTodoAsync(int id, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId, trackChanges: true);
            if (todo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            todo.IsDeleted = true;
            todo.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<TodoResponseDto> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId)
        {
            var existingTodo = await _unitOfWork.Todos.GetByIdAsync(id, userId, trackChanges: true);
            if (existingTodo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            existingTodo.Title = dto.Title ?? existingTodo.Title;
            existingTodo.Description = dto.Description ?? existingTodo.Description;
            existingTodo.Priority = dto.Priority ?? existingTodo.Priority;
            existingTodo.DueDate = dto.DueDate.HasValue ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc) : existingTodo.DueDate;
            existingTodo.IsCompleted = dto.IsCompleted ?? existingTodo.IsCompleted;
            existingTodo.IsHidden = dto.IsHidden ?? existingTodo.IsHidden;
            
            if (dto.CategoryId.HasValue)
            {
                existingTodo.CategoryId = await ResolveCategoryIdAsync(dto.CategoryId.Value, userId);
            }

            existingTodo.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            var updatedTodo = await _unitOfWork.Todos.GetByIdAsync(existingTodo.Id, userId, trackChanges: false);
            return (updatedTodo ?? existingTodo).ToResponseDto()!;
        }

        private async Task<int> ResolveCategoryIdAsync(int inputCategoryId, int userId)
        {
            var userCategories = (await _unitOfWork.Categories.GetAllLightAsync(userId)).ToList();

            if (inputCategoryId == 1 || inputCategoryId == 2 || inputCategoryId == 3 || inputCategoryId <= 0)
            {
                int targetCode = inputCategoryId <= 0 ? 3 : inputCategoryId;
                string targetName = targetCode == 1 ? "Học tập" : (targetCode == 2 ? "Công việc" : "Khác");
                var matchedByName = userCategories.FirstOrDefault(p =>
                    string.Equals(p.Name, targetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name, $"# {targetName}", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(targetName, StringComparison.OrdinalIgnoreCase));

                if (matchedByName != null)
                {
                    return matchedByName.Id;
                }

                var newCategory = new Category { Name = targetName, Color = targetCode == 1 ? "#4a5a3a" : (targetCode == 2 ? "#6b7a45" : "#8a8570"), UserId = userId };
                await _unitOfWork.Categories.AddAsync(newCategory);
                await _unitOfWork.SaveChangesAsync();
                return newCategory.Id;
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(inputCategoryId, userId);
            if (category != null)
            {
                return category.Id;
            }

            throw new BusinessException("Không thể xác định danh mục cho công việc này. Danh mục không tồn tại hoặc bạn không có quyền truy cập.");
        }
    }
}
