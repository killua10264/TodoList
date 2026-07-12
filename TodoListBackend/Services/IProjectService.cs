using TodoListBackend.DTOs.Project;

namespace TodoListBackend.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync(int userId);
        Task<ProjectResponseDto?> GetProjectByIdAsync(int id, int userId);
        Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateDto dto, int userId);
        Task DeleteProjectAsync(int id, int userId);
        Task<ProjectResponseDto> UpdateProjectAsync(int id, ProjectUpdateDto dto, int userId);
    }
}
