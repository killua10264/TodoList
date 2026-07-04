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
        public async Task<IActionResult> GetAllTodos()
        {
            int userId = GetCurrentUserId();
            var todos = await _todoService.GetAllTodosAsync(userId);

            return Ok(todos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoCreateDto dto)
        {
            int userId = GetCurrentUserId();

            var newTodo = await _todoService.CreateTodoAsync(dto, userId);
            if (newTodo == null) return BadRequest(new { message = "Không thể tạo công việc." });

            return StatusCode(201, newTodo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            var updatedTodo = await _todoService.UpdateTodoAsync(id, dto, userId);
            
            if (updatedTodo == null)
            {
                return NotFound(new { message = $"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa."});
            }

            return Ok(updatedTodo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            int userId = GetCurrentUserId();

            var result = await _todoService.DeleteTodoAsync(id, userId);

            if (!result)
            {
                return NotFound(new { message = $"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa."});
            }

            return NoContent();
        }
    }
}