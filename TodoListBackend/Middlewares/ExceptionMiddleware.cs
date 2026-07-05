using System.Net;
using System.Text.Json;
using TodoListBackend.Exceptions;

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

            // FIX 4.1: Chỉ trả message từ BusinessException (do dev kiểm soát).
            // Mọi exception khác → message chung, tránh leak thông tin nội bộ.
            var (statusCode, message) = ex switch
            {
                BusinessException bex => (bex.StatusCode, bex.Message),
                UnauthorizedAccessException => (401, "Không có quyền truy cập."),
                _ => (500, "Đã xảy ra lỗi nội bộ từ máy chủ. Vui lòng thử lại sau.")
            };

            context.Response.StatusCode = statusCode;

            // Cấu trúc JSON trả về cho Client
            var response = new
            {
                statusCode,
                message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
