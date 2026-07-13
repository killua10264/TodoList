using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetAllTodos([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? filter = null, [FromQuery] int? projectId = null, [FromQuery] string? status = null, [FromQuery] string? sortBy = null)
        {
            int userId = GetCurrentUserId();
            var paginatedTodos = await _todoService.GetAllTodosAsync(userId, page, pageSize, filter, projectId, status, sortBy);

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

            return StatusCode(201, newTodo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            var updatedTodo = await _todoService.UpdateTodoAsync(id, dto, userId);
            
            return Ok(updatedTodo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            int userId = GetCurrentUserId();

            await _todoService.DeleteTodoAsync(id, userId);

            return NoContent();
        }
    }
}