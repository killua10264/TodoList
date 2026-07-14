using FluentValidation;
using TodoListBackend.DTOs.Auth;

namespace TodoListBackend.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.GetIdentifier()))
                .WithMessage("Vui lòng nhập Tên đăng nhập hoặc Email.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.");
        }
    }
}
