using AutoMapper;
using DishesApi.DbContexts;
using DishesApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;

namespace DishesApi.EndpointBuilders;

public static class IngredientsEndpointsBuilder
{
    public static void MapIngredientsEndpoints(this IEndpointRouteBuilder app)
    {
        var ingredients = app.MapGroup("/dishes/{dishId:guid}/ingredients");
        ingredients.MapGet("", GetByDishIdAsync);
        // Add more ingredient endpoints here as needed
    }

    public static async Task<Results<NotFound, Ok<List<IngredientDto>>>> GetByDishIdAsync(Guid dishId, DishesDbContext db, IMapper mapper)
    {
        var dish = await db.Dishes.Include(d => d.Ingredients).FirstOrDefaultAsync(d => d.Id == dishId);
        if (dish is null)
            return TypedResults.NotFound();
        var ingredientDtos = mapper.Map<List<IngredientDto>>(dish.Ingredients);
        return TypedResults.Ok(ingredientDtos);
    }
}
