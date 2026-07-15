using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.SubTask;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/subtasks")]
    public class SubTaskController : BaseApiController
    {
        private readonly ISubTaskService _subTaskService;

        public SubTaskController(ISubTaskService subTaskService)
        {
            _subTaskService = subTaskService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubTaskById(int id)
        {
            int userId = GetCurrentUserId();
            var subTask = await _subTaskService.GetSubTaskByIdAsync(id, userId);
            
            if (subTask == null) return NotFound();
            return Ok(subTask);
        }

        [HttpGet("todo/{todoId}")]
        public async Task<IActionResult> GetSubTasksByTodoId(int todoId)
        {
            int userId = GetCurrentUserId();
            var subTasks = await _subTaskService.GetSubTasksByTodoIdAsync(todoId, userId);
            return Ok(subTasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubTask([FromBody] SubTaskCreateDto dto)
        {
            int userId = GetCurrentUserId();
            var newSubTask = await _subTaskService.CreateSubTaskAsync(dto, userId);
            return CreatedAtAction(nameof(GetSubTaskById), new { id = newSubTask.Id }, newSubTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubTask(int id, [FromBody] SubTaskUpdateDto dto)
        {
            int userId = GetCurrentUserId();
            var updatedSubTask = await _subTaskService.UpdateSubTaskAsync(id, dto, userId);
            return Ok(updatedSubTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubTask(int id)
        {
            int userId = GetCurrentUserId();
            await _subTaskService.DeleteSubTaskAsync(id, userId);
            return NoContent();
        }
    }
}
