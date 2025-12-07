using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DishesApi.EndpointBuilders
{
    public static class ErrorsEndpointsBuilder
    {
        public static void MapErrorsEndpoint(this WebApplication app)
        {
            app.Map("/Error", ErrorHandler);
        }

        public static ProblemHttpResult ErrorHandler(HttpContext context, ILogger<ErrorHandlerLoggerCategory> logger)
        {
            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionFeature?.Error;
            if (exception != null)
            {
                logger.LogError(exception, "Unhandled exception occurred.");
            }
            return TypedResults.Problem(
                detail: "An unexpected error occurred. Please try again later.",
                title: "An unexpected error occurred",
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
