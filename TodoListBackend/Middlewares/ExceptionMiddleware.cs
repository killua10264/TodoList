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
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [LỖI HỆ THỐNG] {Message} | Đường dẫn: {Path}", ex.Message, context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            var (statusCode, message) = ex switch
            {
                BusinessException bex => (bex.StatusCode, bex.Message),
                ArgumentException aex => (400, aex.Message),
                KeyNotFoundException kex => (404, kex.Message),
                UnauthorizedAccessException uex => (401, !string.IsNullOrEmpty(uex.Message) && uex.Message != "Attempted to perform an unauthorized operation." ? uex.Message : "Không có quyền truy cập."),
                _ => (500, "Đã xảy ra lỗi nội bộ từ máy chủ. Vui lòng thử lại sau.")
            };

            context.Response.StatusCode = statusCode;
            var response = new
            {
                statusCode,
                message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

