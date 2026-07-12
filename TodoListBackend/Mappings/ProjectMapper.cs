using TodoListBackend.Models;
using TodoListBackend.DTOs.Project;

namespace TodoListBackend.Mappings
{
    public static class ProjectMapper
    {
        public static ProjectResponseDto? ToResponseDto(this Project? projectModel)
        {
            if (projectModel == null) return null;

            return new ProjectResponseDto
            {
                Id = projectModel.Id,
                Name = projectModel.Name,
                Color = projectModel.Color
            };
        }
    }
}
