using TaskApi.Common.Exceptions;

namespace TaskApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong. {ex.Message}, exception type {ex.GetType()}");

                httpContext.Response.ContentType = "application/json";

                var response = httpContext.Response;
                var errorResponse = new
                {
                    error = ex.Message
                };

                switch (ex)
                {
                    case NotFoundException:
                        response.StatusCode = StatusCodes.Status404NotFound;
                        break;

                    default:
                        response.StatusCode = StatusCodes.Status500InternalServerError;
                        errorResponse = new { error = "An unexpected error occurred" };
                        break;
                }

                await response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
