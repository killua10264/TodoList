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
                .MaximumLength(100).WithMessage("Tiêu đề không được vượt quá 100 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự.");

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 5).WithMessage("Độ ưu tiên là các số từ 1 đến 5.");

            RuleFor(x => x.DueDate.Date)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Giờ hết hạn không được là thời gian trong quá khứ.");
        }
    }
}