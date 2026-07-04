using System.Net;
using System.Text.Json;

namespace TodoListBackend.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi tiếp đến các Middleware tiếp theo (Authentication, Controller...)
                await _next(context);
            }
            catch (Exception ex)
            {
                // Bắt toàn bộ lỗi sập/ngoại lệ bị rò rỉ ra từ Controller hoặc Service
                _logger.LogError(ex, "❌ [LỖI HỆ THỐNG] {Message} | Đường dẫn: {Path}", ex.Message, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
                ArgumentException => (int)HttpStatusCode.BadRequest,             // 400
                KeyNotFoundException => (int)HttpStatusCode.NotFound,            // 404
                _ => (int)HttpStatusCode.InternalServerError                     // 500 (Mặc định)
            };

            context.Response.StatusCode = statusCode;

            // Cấu trúc JSON trả về cho Client
            var response = new
            {
                statusCode = statusCode,
                message = statusCode == 500 
                    ? "Đã xảy ra lỗi nội bộ từ máy chủ. Vui lòng thử lại sau." 
                    : ex.Message // Với lỗi 400, 404, 401 thì trả về thông báo lỗi chi tiết cho FE biết
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
