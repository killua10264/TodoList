using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;
using TodoListBackend.Repositories;
using TodoListBackend.Mappings;

namespace TodoListBackend.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;

        public TodoService(ITodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public async Task<IEnumerable<TodoResponseDto>> GetAllTodosAsync(int userId)
        {
            return await _todoRepository.GetAllTodosAsync(userId);
        }

        public async Task<TodoResponseDto?> GetTodoByIdAsync(int id, int userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id, userId);
            if (todo == null) return null;
            return todo.ToResponseDto();
        }

        public async Task<TodoResponseDto?> CreateTodoAsync(TodoCreateDto dto, int userId)
        {
            if (dto.DueDate < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Ngày hết hạn không được nằm trong quá khứ.");
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

            await _todoRepository.AddAsync(newTodo);
            await _todoRepository.SaveChangesAsync();

            return newTodo.ToResponseDto();
        }

        public async Task<bool> DeleteTodoAsync(int id, int userId)
        {
            var todo = await _todoRepository.GetByIdAsync(id, userId);
            if (todo == null) return false;

            todo.IsDeleted = true;
            todo.UpdatedAt = DateTime.UtcNow;

            await _todoRepository.UpdateAsync(todo);
            await _todoRepository.SaveChangesAsync();

            return true;
        }

        public async Task<TodoResponseDto?> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId)
        {
            var existingTodo = await _todoRepository.GetByIdAsync(id, userId);
            if (existingTodo == null) return null;

            existingTodo.Title = dto.Title ?? existingTodo.Title;
            existingTodo.Description = dto.Description ?? existingTodo.Description;
            existingTodo.Priority = dto.Priority ?? existingTodo.Priority;
            existingTodo.DueDate = dto.DueDate ?? existingTodo.DueDate;
            existingTodo.IsCompleted = dto.IsCompleted ?? existingTodo.IsCompleted;
            existingTodo.CategoryId = dto.CategoryId ?? existingTodo.CategoryId;
            existingTodo.UpdatedAt = DateTime.UtcNow;

            await _todoRepository.UpdateAsync(existingTodo);
            await _todoRepository.SaveChangesAsync();

            return existingTodo.ToResponseDto();
        }
    }
}