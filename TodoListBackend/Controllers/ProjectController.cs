using Microsoft.AspNetCore.Mvc;
using TodoListBackend.DTOs.Project;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/projects")]
    public class ProjectController : BaseApiController
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            int userId = GetCurrentUserId();
            var projects = await _projectService.GetAllProjectsAsync(userId);

            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            int userId = GetCurrentUserId();
            var project = await _projectService.GetProjectByIdAsync(id, userId);
            if (project == null) return NotFound(new { message = "Không tìm thấy dự án." });

            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto dto)
        {
            int userId = GetCurrentUserId();

            var newProject = await _projectService.CreateProjectAsync(dto, userId);

            return StatusCode(201, newProject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectUpdateDto dto)
        {
            int userId = GetCurrentUserId();

            var updatedProject = await _projectService.UpdateProjectAsync(id, dto, userId);

            return Ok(updatedProject);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            int userId = GetCurrentUserId();

            await _projectService.DeleteProjectAsync(id, userId);

            return NoContent();
        }
    }
}
