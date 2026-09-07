using Microsoft.AspNetCore.Mvc;
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
                var error = MapException(ex);

                if (error.StatusCode >= StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(
                        ex,
                        "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                }
                else
                {
                    _logger.LogWarning(
                        "Request rejected with {StatusCode} ({ErrorCode}) for {Method} {Path}. TraceId: {TraceId}",
                        error.StatusCode,
                        error.Code,
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                }

                await WriteProblemDetailsAsync(context, error);
            }
        }

        private static MappedError MapException(Exception ex)
        {
            return ex switch
            {
                BusinessException businessException => new MappedError(
                    businessException.StatusCode,
                    GetTitle(businessException.StatusCode),
                    businessException.Message,
                    businessException.Code),
                ArgumentException argumentException => new MappedError(
                    StatusCodes.Status400BadRequest,
                    "Yêu cầu không hợp lệ",
                    argumentException.Message,
                    "invalid_argument"),
                KeyNotFoundException keyNotFoundException => new MappedError(
                    StatusCodes.Status404NotFound,
                    "Không tìm thấy tài nguyên",
                    keyNotFoundException.Message,
                    "resource_not_found"),
                UnauthorizedAccessException => new MappedError(
                    StatusCodes.Status401Unauthorized,
                    "Chưa được xác thực",
                    "Thông tin xác thực không hợp lệ hoặc đã hết hạn.",
                    "unauthorized"),
                _ => new MappedError(
                    StatusCodes.Status500InternalServerError,
                    "Lỗi máy chủ",
                    "Đã xảy ra lỗi nội bộ từ máy chủ. Vui lòng thử lại sau.",
                    "internal_server_error")
            };
        }

        private static async Task WriteProblemDetailsAsync(HttpContext context, MappedError error)
        {
            context.Response.StatusCode = error.StatusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = error.StatusCode,
                Title = error.Title,
                Detail = error.Detail,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["code"] = error.Code;
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            // Compatibility field for the current Angular client. Remove after
            // every caller has migrated from error.message to ProblemDetails.detail.
            problemDetails.Extensions["message"] = error.Detail;

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problemDetails,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                context.RequestAborted);
        }

        private static string GetTitle(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "Yêu cầu không hợp lệ",
            StatusCodes.Status401Unauthorized => "Chưa được xác thực",
            StatusCodes.Status403Forbidden => "Không có quyền truy cập",
            StatusCodes.Status404NotFound => "Không tìm thấy tài nguyên",
            StatusCodes.Status409Conflict => "Dữ liệu xung đột",
            _ => "Không thể xử lý yêu cầu"
        };

        private sealed record MappedError(int StatusCode, string Title, string Detail, string Code);
    }
}
