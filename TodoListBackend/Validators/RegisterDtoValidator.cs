using FluentValidation;
using TodoListBackend.DTOs.Auth;

namespace TodoListBackend.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(50).WithMessage("Tên không được vượt quá 50 ký tự.");

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
