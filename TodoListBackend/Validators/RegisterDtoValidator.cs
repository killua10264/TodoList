using FluentValidation;
using TodoListBackend.DTOs.Auth;

namespace TodoListBackend.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        private static readonly string[] ReservedUsernames = new[] { "admin", "root", "system", "moderator", "support", "staff", "superuser", "null", "undefined" };

        public RegisterDtoValidator()
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
                .WithMessage("Tên đăng nhập này chứa từ khóa nhạy cảm hoặc hệ thống không được phép sử dụng.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches(@"[A-Z]").WithMessage("Mật khẩu cần ít nhất 1 chữ hoa.")
                .Matches(@"[\d]").WithMessage("Mật khẩu cần ít nhất 1 chữ số.")
                .Matches(@"[\W_]").WithMessage("Mật khẩu cần ít nhất 1 ký tự đặc biệt.");
        }
    }
}
