using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TodoListBackend.DTOs.Todo;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/todos")]
    public class TodoController : BaseApiController
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodos([FromQuery] TodoQueryDto query)
        {
            int userId = GetCurrentUserId();
            var paginatedTodos = await _todoService.GetAllTodosAsync(userId, query);

            return Ok(paginatedTodos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoById(int id)
        {
            int userId = GetCurrentUserId();
            var todo = await _todoService.GetTodoByIdAsync(id, userId);
            if (todo == null) return NotFound();

            return Ok(todo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoCreateDto dto)
        {
            int userId = GetCurrentUserId();

            var newTodo = await _todoService.CreateTodoAsync(dto, userId);

            return CreatedAtAction(nameof(GetTodoById), new { id = newTodo.Id }, newTodo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            var updatedTodo = await _todoService.UpdateTodoAsync(id, dto, userId);
            
            return Ok(updatedTodo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id, [FromQuery, BindRequired] uint version)
        {
            int userId = GetCurrentUserId();

            await _todoService.DeleteTodoAsync(id, userId, version);

            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreTodo(int id, [FromQuery, BindRequired] uint version)
        {
            int userId = GetCurrentUserId();
            await _todoService.RestoreTodoAsync(id, userId, version);
            return Ok(new { message = "Khôi phục công việc thành công." });
        }

        [HttpDelete("{id}/hard")]
        public async Task<IActionResult> HardDeleteTodo(int id, [FromQuery, BindRequired] uint version)
        {
            int userId = GetCurrentUserId();
            await _todoService.HardDeleteTodoAsync(id, userId, version);
            return NoContent();
        }
    }
}
