namespace TodoListBackend.Exceptions
{
    public class BusinessException : Exception
    {
        public int StatusCode { get; }
        public string Code { get; }

        public BusinessException(
            string message,
            int statusCode = StatusCodes.Status400BadRequest,
            string code = "business_rule_violation") : base(message)
        {
            StatusCode = statusCode;
            Code = code;
        }
    }

    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message, string code = "resource_not_found")
            : base(message, StatusCodes.Status404NotFound, code) { }
    }
}
