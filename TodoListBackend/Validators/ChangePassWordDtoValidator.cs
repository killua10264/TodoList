using FluentValidation;
using TodoListBackend.DTOs.User;

namespace TodoListBackend.Validators
{
    public class ChangePassWordDtoValidator : AbstractValidator<ChangePassWordDto>
    {
        public ChangePassWordDtoValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Vui lòng nhập mật khẩu hiện tại (mật khẩu cũ).");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Vui lòng nhập mật khẩu mới.")
                .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Vui lòng nhập lại xác nhận mật khẩu mới.")
                .Equal(x => x.NewPassword).WithMessage("Mật khẩu xác nhận không khớp với mật khẩu mới.");
        }
    }
}
