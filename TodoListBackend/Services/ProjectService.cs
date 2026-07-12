using TodoListBackend.Models;
using TodoListBackend.DTOs.Project;
using TodoListBackend.Repositories;
using TodoListBackend.Mappings;
using TodoListBackend.Exceptions;

namespace TodoListBackend.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ProjectResponseDto>> GetAllProjectsAsync(int userId)
        {
            var projects = await _unitOfWork.Projects.GetAllProjectsAsync(userId);
            var projectList = projects.ToList();

            if (!projectList.Any())
            {
                var defaultProjects = new List<Project>
                {
                    new Project { Name = "Học tập", Color = "#4a5a3a", UserId = userId },
                    new Project { Name = "Công việc", Color = "#6b7a45", UserId = userId },
                    new Project { Name = "Khác", Color = "#8a8570", UserId = userId }
                };

                foreach (var proj in defaultProjects)
                {
                    await _unitOfWork.Projects.AddAsync(proj);
                }
                await _unitOfWork.SaveChangesAsync();

                projectList = defaultProjects;
            }

            return projectList.Select(p => p.ToResponseDto()!);
        }

        public async Task<ProjectResponseDto?> GetProjectByIdAsync(int id, int userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id, userId);
            return project?.ToResponseDto();
        }

        public async Task<ProjectResponseDto> CreateProjectAsync(ProjectCreateDto dto, int userId)
        {
            var project = new Project
            {
                Name = dto.Name,
                Color = dto.Color,
                UserId = userId
            };

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return project.ToResponseDto()
                ?? throw new BusinessException("Không thể tạo dự án.");
        }

        public async Task DeleteProjectAsync(int id, int userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id, userId);
            if (project == null)
                throw new NotFoundException($"Không tìm thấy dự án có ID = {id}.");

            await _unitOfWork.Projects.DeleteAsync(project);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProjectResponseDto> UpdateProjectAsync(int id, ProjectUpdateDto dto, int userId)
        {
            var existingProject = await _unitOfWork.Projects.GetByIdAsync(id, userId);
            if (existingProject == null)
                throw new NotFoundException($"Không tìm thấy dự án có ID = {id}.");

            existingProject.Name = dto.Name;
            existingProject.Color = dto.Color;

            await _unitOfWork.SaveChangesAsync();

            return existingProject.ToResponseDto()!;
        }
    }
}
