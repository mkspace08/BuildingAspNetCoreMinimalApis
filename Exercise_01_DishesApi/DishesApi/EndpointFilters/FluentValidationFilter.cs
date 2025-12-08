using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace DishesApi.EndpointFilters;

public class FluentValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public FluentValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arg = context.Arguments.FirstOrDefault(a => a is T) as T;
        if (arg == null)
            return Results.BadRequest("Invalid payload.");

        var validationResult = await _validator.ValidateAsync(arg);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            return Results.ValidationProblem(errors);
        }
        return await next(context);
    }
}
