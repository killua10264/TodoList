using TodoListBackend.DTOs.SubTask;
using TodoListBackend.DTOs.User;
using TodoListBackend.Validators;
using Xunit;

namespace TodoListBackend.Tests.Validators;

public class ValidationTests
{
    [Fact]
    public void UserProfileUpdate_AllowsPartialUpdateWithoutEmail()
    {
        var result = new ProfileUpdateDtoValidator().Validate(new ProfileUpdateDto
        {
            Username = "updated_user",
            Bio = "Updated bio"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void SubTaskUpdate_RequiresVersion()
    {
        var result = new SubTaskUpdateDtoValidator().Validate(new SubTaskUpdateDto
        {
            Title = "Leaf",
            IsCompleted = false
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SubTaskUpdateDto.Version));
    }

    [Fact]
    public void UserProfileUpdate_RejectsNullDisplaySettingsWithoutThrowing()
    {
        var result = new ProfileUpdateDtoValidator().Validate(new ProfileUpdateDto
        {
            Username = "updated_user",
            Theme = null!,
            Language = null!,
            FirstDayOfWeek = null!,
            Timezone = null!
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ProfileUpdateDto.Theme));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ProfileUpdateDto.Language));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ProfileUpdateDto.FirstDayOfWeek));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ProfileUpdateDto.Timezone));
    }
}
