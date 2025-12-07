namespace DishesApi.EndpointFilters;

public class DishNotFoundFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        if (result is IStatusCodeHttpResult statusResult && statusResult.StatusCode == StatusCodes.Status404NotFound)
        {
            return TypedResults.Problem(new()
            {
                Status = 404,
                Title = "Dish not found.",
                Detail = "The requested dish does not exist."
            });
        }
        return result;
    }
}
