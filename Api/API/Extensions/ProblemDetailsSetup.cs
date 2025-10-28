using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ProblemDetailsSetup
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                var ex = ctx.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

                switch (ex)
                {
                    case UnauthorizedAccessException:
                        ctx.ProblemDetails.Status = StatusCodes.Status401Unauthorized;
                        ctx.ProblemDetails.Title = "Unauthorized";
                        break;

                    case ForbiddenException:
                        ctx.ProblemDetails.Status = StatusCodes.Status403Forbidden;
                        ctx.ProblemDetails.Title = "Forbidden";
                        break;

                    case NotFoundException:
                        ctx.ProblemDetails.Status = StatusCodes.Status404NotFound;
                        ctx.ProblemDetails.Title = "Not Found";
                        break;

                    default:
                        ctx.ProblemDetails.Status ??= StatusCodes.Status500InternalServerError;
                        ctx.ProblemDetails.Title ??= "An error occurred while processing your request.";
                        break;
                }

                // Add a correlation id without leaking exception details.
                var traceId = ctx.HttpContext.TraceIdentifier;
                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    ctx.ProblemDetails.Extensions["traceId"] = traceId;
                }
            };
        });

        return services;
    }
}