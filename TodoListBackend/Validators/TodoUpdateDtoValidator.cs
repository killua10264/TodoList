using FluentValidation;
using TodoListBackend.DTOs.Todo;

namespace TodoListBackend.Validators
{
    public class TodoUpdateDtoValidator : AbstractValidator<TodoUpdateDto>
    {
        public TodoUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự.")
                .When(x => x.Title != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Mô tả không được vượt quá 1000 ký tự.")
                .When(x => x.Description != null);

            RuleFor(x => x.Priority)
                .InclusiveBetween(1, 5).WithMessage("Độ ưu tiên là các số từ 1 đến 5.")
                .When(x => x.Priority != null);

            RuleFor(x => x.DueDate!.Value)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Ngày hết hạn không được là ngày trong quá khứ.")
                .When(x => x.DueDate != null);

            RuleFor(x => x.Version)
                .NotNull().WithMessage("Phiên bản công việc là bắt buộc khi cập nhật.");
        }
    }
}
