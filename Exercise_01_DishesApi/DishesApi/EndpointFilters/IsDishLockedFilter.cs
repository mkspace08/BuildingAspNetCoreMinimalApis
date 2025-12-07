using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace DishesApi.EndpointFilters;

public class IsDishLockedFilter : IEndpointFilter
{
    private readonly Guid _lockedDishId;

    public IsDishLockedFilter(Guid lockedDishId)
    {
        _lockedDishId = lockedDishId;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        Guid dishId = context.GetArgument<Guid>(0);
        if (dishId == _lockedDishId)
        {
            return TypedResults.Problem(new()
            {
                Status = 400,
                Title = "Dish is locked and cannot be changed.",
                Detail = "You cannot update or delete a locked dish."
            });
        }
        return await next(context);
    }
}
