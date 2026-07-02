using TodoListBackend.Models;
using TodoListBackend.DTOs.Todo;
using TodoListBackend.Repositories;

namespace TodoListBackend.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _todoRepository;

        public TodoService(ITodoRepository todoRepository)
        {
            _todoRepository = todoRepository;
        }

        public async Task<IEnumerable<Todo>> GetAllTodosAsync()
        {
            return await _todoRepository.GetAllAsync();
        }

        public async Task<Todo?> CreateTodoAsync(TodoCreateDto dto)
        {
            if (dto.DueDate < DateTime.Now.Date)
            {
                throw new ArgumentException("Ngày hết hạn không được nằm trong quá khứ.");
            }

            var newTodo = new Todo
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UserId = dto.UserId, //Tạm thời để test, sau này sẽ lấy từ token
                CategoryId = dto.CategoryId,
                IsCompleted = false,
                IsDeleted = false
            };

            await _todoRepository.AddAsync(newTodo);
            await _todoRepository.SaveChangesAsync();

            return newTodo;
        }

        public async Task<bool> DeleteTodoAsync(int id)
        {
            var todo = await _todoRepository.GetByIdAsync(id);
            if (todo == null) return false;

            todo.IsDeleted = true;
            todo.UpdatedAt = DateTime.Now;

            await _todoRepository.UpdateAsync(todo);
            await _todoRepository.SaveChangesAsync();

            return true;
        }

        public async Task<Todo?> UpdateTodoAsync(int id, TodoUpdateDto dto)
        {
            var existingTodo = await _todoRepository.GetByIdAsync(id);
            if (existingTodo == null) return null;

            existingTodo.Title = dto.Title;
            existingTodo.Description = dto.Description;
            existingTodo.Priority = dto.Priority;
            existingTodo.DueDate = dto.DueDate;
            existingTodo.IsCompleted = dto.IsCompleted;
            existingTodo.CategoryId = dto.CategoryId;
            
            existingTodo.UpdatedAt = DateTime.Now;

            await _todoRepository.UpdateAsync(existingTodo);
            await _todoRepository.SaveChangesAsync();

            return existingTodo;
        }
    }
}