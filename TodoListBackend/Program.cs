using Microsoft.EntityFrameworkCore;
using TodoListBackend.Data;
using TodoListBackend.Repositories;
using TodoListBackend.Services;
using TodoListBackend.Models;

using FluentValidation;
using FluentValidation.AspNetCore;
using TodoListBackend.Validators;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;
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

builder.Services.AddScoped<IPhotoService, PhotoService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthLimit", opt =>
    {
        opt.PermitLimit = 5; // Tối đa 5 request
        opt.Window = TimeSpan.FromMinutes(1); // trong vòng 1 phút
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFE", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
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

app.UseHttpsRedirection();

app.UseCors("AllowFE");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://*:{port}");
}

app.Run();
