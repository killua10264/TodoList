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
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TodoCreateDtoValidator>();

builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISubTaskRepository, SubTaskRepository>();
builder.Services.AddScoped<ISubTaskService, SubTaskService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
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
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] 
                    ?? throw new InvalidOperationException(
                        "JWT signing key is not configured. Use 'dotnet user-secrets set Jwt:Key <your-secret>' or set environment variable 'Jwt__Key'.")))
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
app.Run();