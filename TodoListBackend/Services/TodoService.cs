using Microsoft.EntityFrameworkCore;
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

        public async Task<PaginatedResponse<TodoResponseDto>> GetAllTodosAsync(int userId, TodoQueryDto query)
        {
            if (query.CategoryId.HasValue && query.CategoryId.Value > 0)
            {
                query.CategoryId = await ResolveCategoryFilterIdAsync(query.CategoryId.Value, userId);
            }

            var (todos, totalCount) = await _unitOfWork.Todos.GetAllTodosAsync(userId, query);
            return new PaginatedResponse<TodoResponseDto>
            {
                Items = todos.Select(t => t.ToResponseDto()!),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
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
            if (dto.DueDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                throw new BusinessException("Ngày hết hạn không được nằm trong quá khứ.");
            }

            dto.CategoryId = await ResolveCategoryIdAsync(dto.CategoryId, userId);

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
                IsHidden = false,
                IsDeleted = false
            };

            await _unitOfWork.Todos.AddAsync(newTodo);
            await _unitOfWork.SaveChangesAsync();

            var createdTodo = await _unitOfWork.Todos.GetByIdAsync(newTodo.Id, userId);
            return createdTodo?.ToResponseDto()
                ?? throw new BusinessException("Không thể tạo công việc.");
        }

        public async Task DeleteTodoAsync(int id, int userId, uint expectedVersion)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId, trackChanges: true);
            if (todo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            EnsureVersion(todo.Version, expectedVersion);

            todo.IsDeleted = true;
            todo.UpdatedAt = DateTime.UtcNow;

            await SaveWithConcurrencyHandlingAsync();
        }

        public async Task RestoreTodoAsync(int id, int userId, uint expectedVersion)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId, trackChanges: true, includeDeleted: true);
            if (todo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id}.");

            EnsureVersion(todo.Version, expectedVersion);

            todo.IsDeleted = false;
            todo.UpdatedAt = DateTime.UtcNow;

            await SaveWithConcurrencyHandlingAsync();
        }

        public async Task HardDeleteTodoAsync(int id, int userId, uint expectedVersion)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId, trackChanges: true, includeDeleted: true);
            if (todo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id}.");

            EnsureVersion(todo.Version, expectedVersion);

            _unitOfWork.Todos.Remove(todo);
            await SaveWithConcurrencyHandlingAsync();
        }

        public async Task<TodoResponseDto> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId)
        {
            var existingTodo = await _unitOfWork.Todos.GetByIdAsync(id, userId, trackChanges: true);
            if (existingTodo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            EnsureVersion(existingTodo.Version, dto.Version);

            existingTodo.Title = dto.Title ?? existingTodo.Title;
            existingTodo.Description = dto.Description ?? existingTodo.Description;
            existingTodo.Priority = dto.Priority ?? existingTodo.Priority;
            existingTodo.DueDate = dto.DueDate ?? existingTodo.DueDate;
            existingTodo.IsCompleted = dto.IsCompleted ?? existingTodo.IsCompleted;
            existingTodo.IsHidden = dto.IsHidden ?? existingTodo.IsHidden;
            
            if (dto.CategoryId.HasValue)
            {
                existingTodo.CategoryId = await ResolveCategoryIdAsync(dto.CategoryId.Value, userId);
            }

            existingTodo.UpdatedAt = DateTime.UtcNow;

            await SaveWithConcurrencyHandlingAsync();

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

        private async Task<int> ResolveCategoryFilterIdAsync(int inputCategoryId, int userId)
        {
            var categories = (await _unitOfWork.Categories.GetAllLightAsync(userId)).ToList();
            if (categories.Any(category => category.Id == inputCategoryId))
            {
                return inputCategoryId;
            }

            var targetName = inputCategoryId switch
            {
                1 => "Học tập",
                2 => "Công việc",
                3 => "Khác",
                _ => null
            };

            if (targetName is null)
            {
                return -1;
            }

            var matchingCategory = categories.FirstOrDefault(category =>
                string.Equals(category.Name, targetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(category.Name, $"# {targetName}", StringComparison.OrdinalIgnoreCase) ||
                category.Name.EndsWith(targetName, StringComparison.OrdinalIgnoreCase));

            return matchingCategory?.Id ?? -1;
        }

        private static void EnsureVersion(uint currentVersion, uint? expectedVersion)
        {
            if (expectedVersion.HasValue && expectedVersion.Value != currentVersion)
            {
                throw new BusinessException(
                    "Công việc đã được thay đổi bởi một phiên khác. Vui lòng tải lại dữ liệu.",
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
                    "Công việc đã được thay đổi bởi một phiên khác. Vui lòng tải lại dữ liệu.",
                    StatusCodes.Status409Conflict,
                    "concurrency_conflict");
            }
        }
    }
}
