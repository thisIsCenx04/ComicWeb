using ComicWeb.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ComicWeb.Api.Filters;

public sealed class ApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            return;
        }

        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is IApiResponse)
            {
                return;
            }

            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            objectResult.Value = ApiResponse<object?>.From(objectResult.Value, statusCode);
            objectResult.StatusCode = statusCode;
            return;
        }

        if (context.Result is StatusCodeResult statusCodeResult)
        {
            var statusCode = statusCodeResult.StatusCode;
            context.Result = new ObjectResult(ApiResponse<object?>.From(null, statusCode))
            {
                StatusCode = statusCode
            };
            return;
        }

        if (context.Result is EmptyResult)
        {
            context.Result = new ObjectResult(ApiResponse<object?>.From(null, StatusCodes.Status204NoContent))
            {
                StatusCode = StatusCodes.Status204NoContent
            };
        }
    }
}
