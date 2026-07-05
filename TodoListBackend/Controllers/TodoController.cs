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

        // FIX 2.2: Hỗ trợ pagination qua query string page và pageSize
        [HttpGet]
        public async Task<IActionResult> GetAllTodos([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            int userId = GetCurrentUserId();
            var paginatedTodos = await _todoService.GetAllTodosAsync(userId, page, pageSize);

            return Ok(paginatedTodos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoCreateDto dto)
        {
            int userId = GetCurrentUserId();

            // FIX 4.2: Xóa null-check dead code — Service sẽ throw exception nếu lỗi
            var newTodo = await _todoService.CreateTodoAsync(dto, userId);

            return StatusCode(201, newTodo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            // FIX 4.2: Xóa null-check dead code
            var updatedTodo = await _todoService.UpdateTodoAsync(id, dto, userId);
            
            return Ok(updatedTodo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            int userId = GetCurrentUserId();

            // FIX 4.2: Xóa false check dead code
            await _todoService.DeleteTodoAsync(id, userId);

            return NoContent();
        }
    }
}