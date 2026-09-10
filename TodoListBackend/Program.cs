using Microsoft.EntityFrameworkCore;
using TodoListBackend.Data;
using TodoListBackend.Repositories;
using TodoListBackend.Services;
using TodoListBackend.Models;

using FluentValidation;
using FluentValidation.AspNetCore;
using TodoListBackend.Validators;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
using System.Security.Claims;
using TodoListBackend.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required. Configure it with user secrets or the ConnectionStrings__DefaultConnection environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            var problemDetails = new ValidationProblemDetails(actionContext.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dữ liệu đầu vào không hợp lệ",
                Detail = "Một hoặc nhiều trường dữ liệu không hợp lệ.",
                Instance = actionContext.HttpContext.Request.Path
            };

            problemDetails.Extensions["code"] = "validation_failed";
            problemDetails.Extensions["traceId"] = actionContext.HttpContext.TraceIdentifier;
            problemDetails.Extensions["message"] = problemDetails.Detail;

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Key), "Jwt:Key is required.")
    .Validate(settings => Encoding.UTF8.GetByteCount(settings.Key) >= 32, "Jwt:Key must contain at least 32 bytes.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Issuer), "Jwt:Issuer is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.Audience), "Jwt:Audience is required.")
    .Validate(settings => settings.AccessTokenMinutes is >= 5 and <= 60, "Jwt:AccessTokenMinutes must be between 5 and 60.")
    .ValidateOnStart();

builder.Services.AddOptions<CloudinarySettings>()
    .Bind(builder.Configuration.GetSection(CloudinarySettings.SectionName))
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.CloudName), "CloudinarySettings:CloudName is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.ApiKey), "CloudinarySettings:ApiKey is required.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.ApiSecret), "CloudinarySettings:ApiSecret is required.")
    .ValidateOnStart();

builder.Services.AddOptions<RefreshTokenSettings>()
    .Bind(builder.Configuration.GetSection(RefreshTokenSettings.SectionName))
    .Validate(settings => settings.ExpiryDays is >= 1 and <= 30, "RefreshToken:ExpiryDays must be between 1 and 30.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.CookieName), "RefreshToken:CookieName is required.")
    .Validate(settings => !settings.CookieName.Contains(';'), "RefreshToken:CookieName cannot contain ';'.")
    .Validate(settings => !string.IsNullOrWhiteSpace(settings.CookiePath) && settings.CookiePath.StartsWith('/'), "RefreshToken:CookiePath must start with '/'.")
    .Validate(settings => settings.SameSite is "Strict" or "Lax" or "None", "RefreshToken:SameSite must be Strict, Lax or None.")
    .Validate(settings => settings.SameSite != "None" || settings.Secure, "RefreshToken:Secure must be true when SameSite is None.")
    .Validate(settings => !settings.CookieName.StartsWith("__Host-", StringComparison.Ordinal) ||
        (settings.Secure && settings.CookiePath == "/" && string.IsNullOrWhiteSpace(settings.Domain)),
        "A __Host- refresh cookie must be Secure, use Path=/ and omit Domain.")
    .ValidateOnStart();

var corsSettings = builder.Configuration
    .GetSection(CorsSettings.SectionName)
    .Get<CorsSettings>() ?? new CorsSettings();

var allowedOrigins = corsSettings.AllowedOrigins
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (allowedOrigins.Any(origin =>
        !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)))
{
    throw new InvalidOperationException("Every Cors:AllowedOrigins value must be an absolute HTTP or HTTPS origin.");
}

if (!builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one trusted production origin.");
}

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TodoCreateDtoValidator>();

builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubTaskRepository, SubTaskRepository>();
builder.Services.AddScoped<ISubTaskService, SubTaskService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRefreshTokenSessionRepository, RefreshTokenSessionRepository>();

builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LoginLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientIp(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("RefreshLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientIp(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("RegisterLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientIp(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("AvatarLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetAuthenticatedUserOrClientIp(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(10),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

static string GetClientIp(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

static string GetAuthenticatedUserOrClientIp(HttpContext context)
{
    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    return string.IsNullOrWhiteSpace(userId)
        ? $"ip:{GetClientIp(context)}"
        : $"user:{userId}";
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<Microsoft.Extensions.Options.IOptions<JwtSettings>>((options, jwtOptions) =>
    {
        var settings = jwtOptions.Value;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

var app = builder.Build();

app.UseMiddleware<TodoListBackend.Middlewares.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TestServer does not provide an HTTPS endpoint. Keeping the redirect disabled in
// the test environment makes integration tests exercise the actual API pipeline
// without weakening production HTTPS enforcement.
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFE");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://*:{port}");
}

app.Run();

public partial class Program { }
