using ConvocadoFc.Domain.Models.Modules.Users.Identity;
using ConvocadoFc.Infrastructure.Modules.Authentication;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ConvocadoFc.Infrastructure.Tests.Authentication;

public sealed class RegexPasswordValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenPasswordInvalid_ReturnsFailed()
    {
        var validator = new RegexPasswordValidator();
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@local", FullName = "User" };

        var result = await validator.ValidateAsync(CreateUserManager(), user, "weak");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "PasswordRegex");
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordValid_ReturnsSuccess()
    {
        var validator = new RegexPasswordValidator();
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@local", FullName = "User" };

        var result = await validator.ValidateAsync(CreateUserManager(), user, "Password123!");

        Assert.True(result.Succeeded);
    }

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new UserManager<ApplicationUser>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
