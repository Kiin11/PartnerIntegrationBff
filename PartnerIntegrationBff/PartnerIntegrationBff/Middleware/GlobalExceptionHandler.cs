using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PartnerIntegrationBff.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public GlobalExceptionHandler() { }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                ValidationException valEx => (
                    StatusCodes.Status400BadRequest,
                    "Validation Error",
                    string.Join("; ", valEx.Errors.Select(e => e.ErrorMessage))
                ),
                TimeoutException => (
                    StatusCodes.Status504GatewayTimeout,
                    "Gateway Timeout",
                    "An external downstream dependency timed out."
                ),
                HttpRequestException => (
                    StatusCodes.Status502BadGateway,
                    "Bad Gateway",
                    "An error occurred while communicating with an upstream service."
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected internal error occurred."
                )
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
