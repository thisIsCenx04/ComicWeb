using ComicWeb.Application.DTOs;
using Microsoft.AspNetCore.Diagnostics;

namespace ComicWeb.Api.Filters;

public static class ExceptionHandlerExtensions
{
    /// <summary>
    /// Registers a global exception handler that returns standardized responses.
    /// </summary>
    public static void UseApiExceptionHandler(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(handlerApp =>
        {
            handlerApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var statusCode = StatusCodes.Status500InternalServerError;

                if (exceptionHandlerPathFeature?.Error is KeyNotFoundException)
                {
                    statusCode = StatusCodes.Status404NotFound;
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                logger.LogError(exceptionHandlerPathFeature?.Error, "Unhandled exception");

                var response = ApiResponse<object?>.From(null, statusCode, exceptionHandlerPathFeature?.Error.Message);
                await context.Response.WriteAsJsonAsync(response);
            });
        });
    }
}
