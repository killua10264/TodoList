using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodos()
        {
            var todos = await _todoService.GetAllTodosAsync();
            return Ok(todos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoCreateDto dto)
        {
            try
            {
                var newTodo = await _todoService.CreateTodoAsync(dto);
                return StatusCode(201, newTodo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoUpdateDto dto)
        {
            try 
            {
                var updatedTodo = await _todoService.UpdateTodoAsync(id, dto);
                
                if (updatedTodo == null)
                {
                    return NotFound(new { message = $"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa."});
                }

                return Ok(updatedTodo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            try 
            {
                var result = await _todoService.SoftDeleteTodoAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa."});
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}