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
                .MinimumLength(8).WithMessage("Mật khẩu mới phải có ít nhất 8 ký tự.")
                .Matches(@"[A-Z]").WithMessage("Mật khẩu mới cần ít nhất 1 chữ hoa.")
                .Matches(@"[\d]").WithMessage("Mật khẩu mới cần ít nhất 1 chữ số.")
                .Matches(@"[\W_]").WithMessage("Mật khẩu mới cần ít nhất 1 ký tự đặc biệt.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Vui lòng nhập lại xác nhận mật khẩu mới.")
                .Equal(x => x.NewPassword).WithMessage("Mật khẩu xác nhận không khớp với mật khẩu mới.");
        }
    }
}
