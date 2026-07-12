using TodoListBackend.Models;
using TodoListBackend.DTOs;
using TodoListBackend.DTOs.Todo;
using TodoListBackend.Repositories;
using TodoListBackend.Mappings;
using TodoListBackend.Exceptions;

namespace TodoListBackend.Services
{
    public class TodoService : ITodoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TodoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResponse<TodoResponseDto>> GetAllTodosAsync(int userId, int page = 1, int pageSize = 20, string? filter = null, int? projectId = null, string? status = null, string? sortBy = null)
        {
            if (projectId.HasValue && projectId.Value > 0)
            {
                projectId = await ResolveProjectIdAsync(projectId.Value, userId);
            }

            var (todos, totalCount) = await _unitOfWork.Todos.GetAllTodosAsync(userId, page, pageSize, filter, projectId, status, sortBy);
            return new PaginatedResponse<TodoResponseDto>
            {
                Items = todos.Select(t => t.ToResponseDto()!),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<TodoResponseDto?> GetTodoByIdAsync(int id, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId);
            if (todo == null) return null;
            return todo.ToResponseDto();
        }

        public async Task<TodoResponseDto> CreateTodoAsync(TodoCreateDto dto, int userId)
        {
            if (dto.DueDate < DateTime.UtcNow.Date)
            {
                throw new BusinessException("Ngày hết hạn không được nằm trong quá khứ.");
            }

            dto.ProjectId = await ResolveProjectIdAsync(dto.ProjectId, userId);

            var newTodo = new Todo
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId,
                ProjectId = dto.ProjectId,
                IsCompleted = false,
                IsDeleted = false
            };

            await _unitOfWork.Todos.AddAsync(newTodo);
            await _unitOfWork.SaveChangesAsync();

            var createdTodo = await _unitOfWork.Todos.GetByIdAsync(newTodo.Id, userId);
            return createdTodo?.ToResponseDto()
                ?? throw new BusinessException("Không thể tạo công việc.");
        }

        public async Task DeleteTodoAsync(int id, int userId)
        {
            var todo = await _unitOfWork.Todos.GetByIdAsync(id, userId);
            if (todo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            todo.IsDeleted = true;
            todo.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<TodoResponseDto> UpdateTodoAsync(int id, TodoUpdateDto dto, int userId)
        {
            var existingTodo = await _unitOfWork.Todos.GetByIdAsync(id, userId);
            if (existingTodo == null)
                throw new NotFoundException($"Không tìm thấy công việc có ID = {id} hoặc công việc đã bị xóa.");

            existingTodo.Title = dto.Title ?? existingTodo.Title;
            existingTodo.Description = dto.Description ?? existingTodo.Description;
            existingTodo.Priority = dto.Priority ?? existingTodo.Priority;
            existingTodo.DueDate = dto.DueDate ?? existingTodo.DueDate;
            existingTodo.IsCompleted = dto.IsCompleted ?? existingTodo.IsCompleted;
            
            if (dto.ProjectId.HasValue)
            {
                existingTodo.ProjectId = await ResolveProjectIdAsync(dto.ProjectId.Value, userId);
            }

            existingTodo.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            var updatedTodo = await _unitOfWork.Todos.GetByIdAsync(existingTodo.Id, userId);
            return (updatedTodo ?? existingTodo).ToResponseDto()!;
        }

        private async Task<int> ResolveProjectIdAsync(int inputProjectId, int userId)
        {
            var userProjects = (await _unitOfWork.Projects.GetAllProjectsAsync(userId)).ToList();

            if (inputProjectId == 1 || inputProjectId == 2 || inputProjectId == 3)
            {
                string targetName = inputProjectId == 1 ? "Học tập" : (inputProjectId == 2 ? "Công việc" : "Khác");
                var matchedByName = userProjects.FirstOrDefault(p =>
                    string.Equals(p.Name, targetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name, $"# {targetName}", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.EndsWith(targetName, StringComparison.OrdinalIgnoreCase));

                if (matchedByName != null)
                {
                    return matchedByName.Id;
                }

                var newProject = new Project { Name = targetName, Color = "#4a5a3a", UserId = userId };
                await _unitOfWork.Projects.AddAsync(newProject);
                await _unitOfWork.SaveChangesAsync();
                return newProject.Id;
            }

            var project = await _unitOfWork.Projects.GetByIdAsync(inputProjectId, userId);
            if (project != null)
            {
                return project.Id;
            }

            return userProjects.FirstOrDefault()?.Id ?? inputProjectId;
        }
    }
}