using FluentValidation;
using TodoListBackend.DTOs.Project;

namespace TodoListBackend.Validators
{
    public class ProjectUpdateDtoValidator : AbstractValidator<ProjectUpdateDto>
    {
        public ProjectUpdateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên dự án không được để trống.")
                .MaximumLength(100).WithMessage("Tên dự án tối đa 100 ký tự.");

            RuleFor(x => x.Color)
                .MaximumLength(30).WithMessage("Màu dự án tối đa 30 ký tự.");
        }
    }
}
