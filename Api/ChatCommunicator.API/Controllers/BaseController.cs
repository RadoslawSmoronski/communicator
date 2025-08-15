using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatCommunicator.API.Controllers
{
    public abstract class BaseController : Controller
    {

        protected IActionResult ValidateAndGetUserId(string logContext, ILogger logger, out Guid userId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
            {
                userId = Guid.Empty;
                logger.LogWarning("[{LogContext}] Missing or invalid UserId claim. Returning 401 Unauthorized.", logContext);
                return Problem(
                    statusCode: 401,
                    title: "Unauthorized",
                    detail: "Authentication is required. Missing or invalid user identifier."
                );
            }

            return null!;
        }

        protected IActionResult HandleError(Result result, string logContext, ILogger logger, Guid? userId = null)
        {
            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                switch (errorCode)
                {
                    case ErrorType.Validation:
                        logger.LogWarning("[{LogContext}] Validation error. UserId: {UserId}. Message: {ErrorMessage}", logContext, userId, errorMessage);
                        return Problem(
                            statusCode: 400,
                            title: "Bad Request",
                            detail: errorMessage
                        );

                    case ErrorType.Unauthorized:
                        logger.LogWarning("[{LogContext}] Unauthorized access. UserId: {UserId}. Message: {ErrorMessage}", logContext, userId, errorMessage);
                        return Problem(
                            statusCode: 401,
                            title: "Unauthorized",
                            detail: errorMessage
                        );

                    case ErrorType.NotFound:
                        logger.LogWarning("[{LogContext}] Resource not found. UserId: {UserId}. Message: {ErrorMessage}", logContext, userId, errorMessage);
                        return Problem(
                            statusCode: 404,
                            title: "Not Found",
                            detail: errorMessage
                        );

                    case ErrorType.Conflict:
                        logger.LogWarning("[{LogContext}] Conflict error. UserId: {UserId}. Message: {ErrorMessage}", logContext, userId, errorMessage);
                        return Problem(
                            statusCode: 409,
                            title: "Conflict",
                            detail: errorMessage
                        );

                    default:
                        logger.LogError("[{LogContext}] Unexpected error. UserId: {UserId}. Message: {ErrorMessage}", logContext, userId, errorMessage);
                        return Problem(
                            statusCode: 500,
                            title: "Internal Server Error",
                            detail: "An unexpected error occurred."
                        );
                }
            }

            logger.LogError("[{LogContext}] Unexpected null error object. UserId: {UserId}", logContext, userId);
            return Problem(
                statusCode: 500,
                title: "Internal Server Error",
                detail: "An unexpected error occurred."
            );
        }

    }
}
