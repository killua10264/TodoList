using FluentValidation;
using TodoListBackend.DTOs.SubTask;

namespace TodoListBackend.Validators
{
    public class SubTaskUpdateDtoValidator : AbstractValidator<SubTaskUpdateDto>
    {
        public SubTaskUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề công việc con không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");
        }
    }
}
