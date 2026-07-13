using FluentValidation;
using TodoListBackend.DTOs.User;

namespace TodoListBackend.Validators
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
                .MaximumLength(50).WithMessage("Tên đăng nhập tối đa 50 ký tự.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không đúng.");

            RuleFor(x => x.DisplayName)
                .MaximumLength(100).WithMessage("Tên hiển thị tối đa 100 ký tự.");

            RuleFor(x => x.Bio)
                .MaximumLength(300).WithMessage("Dòng trạng thái tối đa 300 ký tự.");

            RuleFor(x => x.Theme)
                .Must(theme => string.IsNullOrEmpty(theme) || new[] { "light", "dark", "system" }.Contains(theme.ToLower()))
                .WithMessage("Chế độ hiển thị không hợp lệ (light, dark, system).");

            RuleFor(x => x.Language)
                .Must(lang => string.IsNullOrEmpty(lang) || new[] { "vi", "en" }.Contains(lang.ToLower()))
                .WithMessage("Ngôn ngữ không hợp lệ (vi, en).");

            RuleFor(x => x.FirstDayOfWeek)
                .Must(day => string.IsNullOrEmpty(day) || new[] { "Monday", "Sunday" }.Contains(day))
                .WithMessage("Ngày bắt đầu tuần không hợp lệ (Monday, Sunday).");
        }
    }
}