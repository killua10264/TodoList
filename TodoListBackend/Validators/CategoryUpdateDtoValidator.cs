using FluentValidation;
using TodoListBackend.DTOs.Category;

namespace TodoListBackend.Validators
{
    public class CategoryUpdateDtoValidator : AbstractValidator<CategoryUpdateDto>
    {
        public CategoryUpdateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên danh mục không được để trống.")
                .MaximumLength(100).WithMessage("Tên danh mục tối đa 100 ký tự.");

            RuleFor(x => x.Color)
                .MaximumLength(30).WithMessage("Màu danh mục tối đa 30 ký tự.");
        }
    }
}
