using FluentValidation;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Validators
{
    public class TodoCreateDtoValidator : AbstractValidator<TodoCreateDto>
    {
        public TodoCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự.");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 5).WithMessage("Độ ưu tiên là các số từ 1 đến 5.");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Ngày hết hạn không được là ngày trong quá khứ.");
        }
    }
}
