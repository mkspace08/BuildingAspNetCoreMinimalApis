using AutoMapper;
using DishesApi.DbContexts;
using DishesApi.Models;
using DishesApi.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;

namespace DishesApi.EndpointBuilders;

public static class DishesEndpointsBuilder
{
    public static void RegisterDishesEndpoints(this IEndpointRouteBuilder app)
    {
        var dishes = app.MapGroup("/dishes");
        dishes.MapGet("", GetAllAsync);
        dishes.MapGet("/{name}", GetByNameAsync);
        dishes.MapGet("/{id:guid}", GetByIdAsync).WithName("GetDish");
        dishes.MapPost("", CreateAsync);
        dishes.MapPut("/{id:guid}", UpdateAsync);
        dishes.MapDelete("/{id:guid}", DeleteAsync);
    }

    public static async Task<Ok<List<DishDto>>> GetAllAsync(DishesDbContext db, IMapper mapper)
    {
        var dishes = await db.Dishes.ToListAsync();
        var dishDtos = mapper.Map<List<DishDto>>(dishes);
        return TypedResults.Ok(dishDtos);
    }

    public static async Task<Results<NotFound, Ok<DishDto>>> GetByNameAsync(string name, DishesDbContext db, ClaimsPrincipal user, IMapper mapper)
    {
        var lowered = name.ToLower();
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Name.ToLower().Contains(lowered));
        if (dish is null)
            return TypedResults.NotFound();
        var dishDto = mapper.Map<DishDto>(dish);
        return TypedResults.Ok(dishDto);
    }

    public static async Task<Results<NotFound, Ok<DishDto>>> GetByIdAsync(Guid id, DishesDbContext db, IMapper mapper)
    {
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Id == id);
        if (dish is null)
            return TypedResults.NotFound();
        var dishDto = mapper.Map<DishDto>(dish);
        return TypedResults.Ok(dishDto);
    }

    public static async Task<Results<BadRequest, CreatedAtRoute<DishDto>>> CreateAsync(CreateDishDto dto, DishesDbContext db, IMapper mapper)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return TypedResults.BadRequest();
        var dishEntity = mapper.Map<Dish>(dto);
        dishEntity.Id = Guid.NewGuid();
        db.Dishes.Add(dishEntity);
        await db.SaveChangesAsync();
        var createdDto = mapper.Map<DishDto>(dishEntity);
        return TypedResults.CreatedAtRoute(createdDto, "GetDish", new { id = createdDto.Id });
    }

    public static async Task<Results<NotFound, NoContent>> UpdateAsync(Guid id, UpdateDishDto dto, DishesDbContext db, IMapper mapper)
    {
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Id == id);
        if (dish is null)
            return TypedResults.NotFound();
        mapper.Map(dto, dish);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    public static async Task<Results<NotFound, NoContent>> DeleteAsync(Guid id, DishesDbContext db)
    {
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Id == id);
        if (dish is null)
            return TypedResults.NotFound();
        db.Dishes.Remove(dish);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}
