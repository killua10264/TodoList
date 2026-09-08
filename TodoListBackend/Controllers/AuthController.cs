using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TodoListBackend.DTOs.Auth;
using TodoListBackend.Exceptions;
using TodoListBackend.Options;
using TodoListBackend.Services;
using Microsoft.Extensions.Options;

namespace TodoListBackend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly RefreshTokenSettings _refreshTokenSettings;

        public AuthController(
            IAuthService authService,
            IOptions<RefreshTokenSettings> refreshTokenOptions)
        {
            _authService = authService;
            _refreshTokenSettings = refreshTokenOptions.Value;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("RegisterLimit")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _authService.RegisterAsync(request, GetSessionContext());
            SetRefreshTokenCookie(result.RefreshToken);
            return StatusCode(201, ToResponse(result));
        }
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("LoginLimit")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await _authService.LoginAsync(request, GetSessionContext());
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(ToResponse(result));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [EnableRateLimiting("RefreshLimit")]
        public async Task<IActionResult> RefreshToken()
        {
            RequireCsrfHeader();
            var refreshToken = Request.Cookies[_refreshTokenSettings.CookieName];
            var result = await _authService.RefreshTokenAsync(refreshToken, GetSessionContext());
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(ToResponse(result));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [EnableRateLimiting("RefreshLimit")]
        public async Task<IActionResult> Logout()
        {
            RequireCsrfHeader();
            int? userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
            await _authService.LogoutAsync(
                Request.Cookies[_refreshTokenSettings.CookieName],
                userId);
            Response.Cookies.Delete(_refreshTokenSettings.CookieName, BuildCookieOptions());
            return NoContent();
        }

        [HttpPost("logout-all")]
        [EnableRateLimiting("RefreshLimit")]
        public async Task<IActionResult> LogoutAll()
        {
            RequireCsrfHeader();
            await _authService.LogoutAllAsync(GetCurrentUserId());
            Response.Cookies.Delete(_refreshTokenSettings.CookieName, BuildCookieOptions());
            return NoContent();
        }

        private void RequireCsrfHeader()
        {
            if (!Request.Headers.TryGetValue("X-CSRF-Protection", out var value) || value != "1")
            {
                throw new BusinessException(
                    "Thiếu header bảo vệ CSRF.",
                    StatusCodes.Status403Forbidden,
                    "csrf_header_required");
            }
        }

        private static AuthResponseDto ToResponse(AuthTokenResult result) => new()
        {
            AccessToken = result.AccessToken,
            ExpiresAt = result.ExpiresAt,
            User = result.User
        };

        private AuthSessionContext GetSessionContext() => new(
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString());

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append(
                _refreshTokenSettings.CookieName,
                refreshToken,
                BuildCookieOptions(DateTimeOffset.UtcNow.AddDays(_refreshTokenSettings.ExpiryDays)));
        }

        private CookieOptions BuildCookieOptions(DateTimeOffset? expires = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = _refreshTokenSettings.Secure,
                SameSite = ParseSameSite(_refreshTokenSettings.SameSite),
                Path = _refreshTokenSettings.CookiePath,
                Domain = string.IsNullOrWhiteSpace(_refreshTokenSettings.Domain)
                    ? null
                    : _refreshTokenSettings.Domain,
                IsEssential = true,
                Expires = expires
            };
        }

        private static SameSiteMode ParseSameSite(string value) => value switch
        {
            "Strict" => SameSiteMode.Strict,
            "None" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
    }
}
