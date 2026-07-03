using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.Todo;
using TodoListBackend.Services;

using Microsoft.AspNetCore.Authorization;

namespace TodoListBackend.Controllers
{
    [Authorize]
    [Route("api/todos")]
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

        var responseList = todos.Select(t => new TodoResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name ?? string.Empty,
                CategoryColor = t.Category?.Color ?? string.Empty
            });

            return Ok(responseList);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoCreateDto dto)
        {
            var newTodo = await _todoService.CreateTodoAsync(dto);
            if (newTodo == null) return BadRequest(new { message = "Không thể tạo công việc." });

            var responseDto = new TodoResponseDto
            {
                Id = newTodo.Id,
                Title = newTodo.Title,
                Description = newTodo.Description,
                IsCompleted = newTodo.IsCompleted,
                Priority = newTodo.Priority,
                DueDate = newTodo.DueDate,
                CategoryId = newTodo.CategoryId,
                CategoryName = newTodo.Category?.Name ?? string.Empty,
                CategoryColor = newTodo.Category?.Color ?? string.Empty
            };

            return StatusCode(201, responseDto);
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

                var responseDto = new TodoResponseDto
                {
                    Id = updatedTodo.Id,
                    Title = updatedTodo.Title,
                    Description = updatedTodo.Description,
                    IsCompleted = updatedTodo.IsCompleted,
                    Priority = updatedTodo.Priority,
                    DueDate = updatedTodo.DueDate,
                    CategoryId = updatedTodo.CategoryId,
                    CategoryName = updatedTodo.Category?.Name ?? string.Empty,
                    CategoryColor = updatedTodo.Category?.Color ?? string.Empty
                };

                return Ok(responseDto);
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
                var result = await _todoService.DeleteTodoAsync(id);

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