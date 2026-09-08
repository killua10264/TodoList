using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TodoListBackend.Controllers;
using TodoListBackend.DTOs.Auth;
using TodoListBackend.Exceptions;
using TodoListBackend.Options;
using TodoListBackend.Services;
using Xunit;

namespace TodoListBackend.Tests.Controllers;

public class AuthControllerTests
{
    private const string CookieName = "todo_refresh_token";

    [Fact]
    public async Task Login_SetsHttpOnlyCookie_AndReturnsOnlyAccessToken()
    {
        var authService = new StubAuthService
        {
            Result = new AuthTokenResult("access-token", "refresh-token")
        };
        var context = CreateContext();
        var controller = CreateController(authService, context);

        var result = await controller.Login(new LoginDto
        {
            UsernameOrEmail = "user@example.com",
            Password = "password"
        });

        var response = Assert.IsType<OkObjectResult>(result);
        var authResponse = Assert.IsType<AuthResponseDto>(response.Value);
        Assert.Equal("access-token", authResponse.AccessToken);
        Assert.Contains(CookieName + "=refresh-token", GetSetCookie(context), StringComparison.Ordinal);
        Assert.Contains("HttpOnly", GetSetCookie(context), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SameSite=Lax", GetSetCookie(context), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Path=/api/auth", GetSetCookie(context), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshToken_ReadsCookie_AndRequiresCsrfHeader()
    {
        var authService = new StubAuthService
        {
            Result = new AuthTokenResult("new-access-token", "new-refresh-token")
        };
        var context = CreateContext();
        context.Request.Headers.Cookie = $"{CookieName}=cookie-refresh-token";
        context.Request.Headers["X-CSRF-Protection"] = "1";
        var controller = CreateController(authService, context);

        var result = await controller.RefreshToken();

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal("cookie-refresh-token", authService.ReceivedRefreshToken);
    }

    [Fact]
    public async Task Logout_RevokesCookieSession_AndDeletesCookie()
    {
        var authService = new StubAuthService();
        var context = CreateContext();
        context.Request.Headers.Cookie = $"{CookieName}=refresh-token";
        context.Request.Headers["X-CSRF-Protection"] = "1";
        var controller = CreateController(authService, context);

        var result = await controller.Logout();

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("refresh-token", authService.ReceivedRefreshToken);
        Assert.Contains(CookieName, GetSetCookie(context), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefreshToken_WithoutCsrfHeader_IsRejected()
    {
        var controller = CreateController(new StubAuthService(), CreateContext());

        var exception = await Assert.ThrowsAsync<BusinessException>(() => controller.RefreshToken());

        Assert.Equal(StatusCodes.Status403Forbidden, exception.StatusCode);
        Assert.Equal("csrf_header_required", exception.Code);
    }

    [Fact]
    public async Task LogoutAll_RequiresAuthenticatedUser_AndDeletesCookie()
    {
        var authService = new StubAuthService();
        var context = CreateContext();
        context.Request.Headers["X-CSRF-Protection"] = "1";
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, "42") },
            authenticationType: "test"));
        var controller = CreateController(authService, context);

        var result = await controller.LogoutAll();

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(42, authService.ReceivedLogoutAllUserId);
        Assert.Contains(CookieName, GetSetCookie(context), StringComparison.Ordinal);
    }

    private static AuthController CreateController(
        StubAuthService authService,
        DefaultHttpContext context)
    {
        var controller = new AuthController(
            authService,
            Microsoft.Extensions.Options.Options.Create(new RefreshTokenSettings
            {
                CookieName = CookieName,
                CookiePath = "/api/auth",
                SameSite = "Lax",
                Secure = false,
                ExpiryDays = 7
            }));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
        return controller;
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.UserAgent = "TodoList.Tests";
        return context;
    }

    private static string GetSetCookie(DefaultHttpContext context) =>
        context.Response.Headers.SetCookie.ToString();

    private sealed class StubAuthService : IAuthService
    {
        public AuthTokenResult Result { get; set; } = new("access-token", "refresh-token");
        public string? ReceivedRefreshToken { get; private set; }
        public int? ReceivedLogoutAllUserId { get; private set; }

        public Task<AuthTokenResult> RegisterAsync(RegisterDto dto, AuthSessionContext sessionContext) =>
            Task.FromResult(Result);

        public Task<AuthTokenResult> LoginAsync(LoginDto dto, AuthSessionContext sessionContext) =>
            Task.FromResult(Result);

        public Task<AuthTokenResult> RefreshTokenAsync(
            string? refreshToken,
            AuthSessionContext sessionContext)
        {
            ReceivedRefreshToken = refreshToken;
            return Task.FromResult(Result);
        }

        public Task LogoutAsync(string? refreshToken, int? userId)
        {
            ReceivedRefreshToken = refreshToken;
            return Task.CompletedTask;
        }

        public Task LogoutAllAsync(int userId)
        {
            ReceivedLogoutAllUserId = userId;
            return Task.CompletedTask;
        }
    }
}
