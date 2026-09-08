using FluentValidation;
using TodoListBackend.DTOs.User;

namespace TodoListBackend.Validators;

public class ProfileUpdateDtoValidator : AbstractValidator<ProfileUpdateDto>
{
    private static readonly string[] ReservedUsernames =
        ["admin", "root", "system", "moderator", "support", "staff", "superuser", "null", "undefined"];

    public ProfileUpdateDtoValidator()
    {
        RuleFor(x => x.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
            .Length(3, 30).WithMessage("Tên đăng nhập phải từ 3 đến 30 ký tự.")
            .Matches(@"^[a-zA-Z0-9_.]+$").WithMessage("Tên đăng nhập chỉ được chứa chữ cái không dấu (a-z), chữ số (0-9), dấu chấm (.) và dấu gạch dưới (_).")
            .Must(u => !u.StartsWith("_") && !u.StartsWith(".") && !u.EndsWith("_") && !u.EndsWith("."))
            .WithMessage("Tên đăng nhập không được bắt đầu hoặc kết thúc bằng dấu chấm (.) hoặc dấu gạch dưới (_).")
            .Must(u => !u.Contains("..") && !u.Contains("__") && !u.Contains("._") && !u.Contains("_."))
            .WithMessage("Tên đăng nhập không được chứa 2 dấu chấm hoặc dấu gạch dưới liên tiếp.")
            .Must(u => !ReservedUsernames.Contains(u.ToLowerInvariant()))
            .WithMessage("Tên đăng nhập này chứa từ khóa nhạy cảm hoặc hệ thống không được phép sử dụng.");

        RuleFor(x => x.Bio)
            .MaximumLength(300).WithMessage("Dòng trạng thái tối đa 300 ký tự.");

        RuleFor(x => x.Theme)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Chế độ hiển thị không được để trống.")
            .Must(theme => theme is not null && new[] { "light", "dark", "system" }.Contains(theme.ToLowerInvariant()))
            .WithMessage("Chế độ hiển thị không hợp lệ (light, dark, system).");

        RuleFor(x => x.Language)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ngôn ngữ không được để trống.")
            .Must(language => language is not null && new[] { "vi", "en" }.Contains(language.ToLowerInvariant()))
            .WithMessage("Ngôn ngữ không hợp lệ (vi, en).");

        RuleFor(x => x.FirstDayOfWeek)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ngày bắt đầu tuần không được để trống.")
            .Must(day => new[] { "Monday", "Sunday" }.Contains(day))
            .WithMessage("Ngày bắt đầu tuần không hợp lệ (Monday, Sunday).");

        RuleFor(x => x.Timezone)
            .NotEmpty().WithMessage("Múi giờ không được để trống.")
            .MaximumLength(100).WithMessage("Múi giờ tối đa 100 ký tự.");
    }
}
