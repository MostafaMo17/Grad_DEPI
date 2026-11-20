using MediCare.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Middleware
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Run the rest of the pipeline (controllers, services, ...)
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            int statusCode;
            string title;
            string detail;

            if (ex is BadRequestException)
            {
                statusCode = 400;
                title = "Bad request";
                detail = ex.Message;
            }
            else if (ex is UnauthorizedException)
            {
                statusCode = 401;
                title = "Unauthorized";
                detail = ex.Message;
            }
            else if (ex is ForbiddenException)
            {
                statusCode = 403;
                title = "Forbidden";
                detail = ex.Message;
            }
            else if (ex is NotFoundException)
            {
                statusCode = 404;
                title = "Not found";
                detail = ex.Message;
            }
            else if (ex is ConflictException)
            {
                statusCode = 409;
                title = "Conflict";
                detail = ex.Message;
            }
            else if (ex is LockedException)
            {
                statusCode = 423;
                title = "Locked";
                detail = ex.Message;
            }
            else
            {
                // Unexpected error: log everything, but show the client only a generic message.
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

                statusCode = 500;
                title = "Server error";
                detail = "An unexpected error occurred. Please try again later.";
            }

            //  the standard JSON error shape (status, title, detail).
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
