using FluentValidation;
using TodoListBackend.DTOs.User;

namespace TodoListBackend.Validators
{
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        private static readonly string[] ReservedUsernames = new[] { "admin", "root", "system", "moderator", "support", "staff", "superuser", "null", "undefined" };

        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
                .Length(3, 30).WithMessage("Tên đăng nhập phải từ 3 đến 30 ký tự.")
                .Matches(@"^[a-zA-Z0-9_.]+$").WithMessage("Tên đăng nhập chỉ được chứa chữ cái không dấu (a-z), chữ số (0-9), dấu chấm (.) và dấu gạch dưới (_).")
                .Must(u => !string.IsNullOrEmpty(u) && !u.StartsWith("_") && !u.StartsWith(".") && !u.EndsWith("_") && !u.EndsWith("."))
                .WithMessage("Tên đăng nhập không được bắt đầu hoặc kết thúc bằng dấu chấm (.) hoặc dấu gạch dưới (_).")
                .Must(u => !string.IsNullOrEmpty(u) && !u.Contains("..") && !u.Contains("__") && !u.Contains("._") && !u.Contains("_."))
                .WithMessage("Tên đăng nhập không được chứa 2 dấu chấm hoặc dấu gạch dưới liên tiếp.")
                .Must(u => !string.IsNullOrEmpty(u) && !ReservedUsernames.Contains(u.ToLower()))
                .WithMessage("Tên đăng nhập này chứa từ khóa nhạy cảm hoặc hệ thống không được phép sử dụng.")
                .When(x => !string.IsNullOrWhiteSpace(x.Username));

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