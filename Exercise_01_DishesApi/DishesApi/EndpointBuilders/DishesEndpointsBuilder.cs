using AutoMapper;
using DishesApi.DbContexts;
using DishesApi.Models;
using DishesApi.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using DishesApi.EndpointFilters;
using FluentValidation;

namespace DishesApi.EndpointBuilders;

public static class DishesEndpointsBuilder
{
    public static void RegisterDishesEndpoints(this IEndpointRouteBuilder app)
    {
        var dishes = app.MapGroup("/dishes");
        dishes.MapGet("", GetAllAsync);
        dishes.MapGet("/{name}", GetByNameAsync);
        dishes.MapGet("/{id:guid}", GetByIdAsync).WithName("GetDish");
        dishes.MapPost("", CreateAsync)
            .AddEndpointFilter<FluentValidationFilter<CreateDishDto>>();
        dishes.MapPut("/{id:guid}", UpdateAsync)
            .AddEndpointFilter<FluentValidationFilter<UpdateDishDto>>()
            //.ProducesValidationProblem()
            .AddEndpointFilter(new IsDishLockedFilter(new Guid("fd630a57-2352-4731-b25c-db9cc7601b16")))
            .AddEndpointFilter<DishNotFoundFilter>();
        dishes.MapDelete("/{id:guid}", DeleteAsync)
            .AddEndpointFilter(new IsDishLockedFilter(new Guid("fd630a57-2352-4731-b25c-db9cc7601b16")))
            .AddEndpointFilter<DishNotFoundFilter>();
    }

    public static async Task<Ok<List<DishDto>>> GetAllAsync(DishesDbContext db, IMapper mapper, [FromServices] ILogger<DishDto> logger)
    {
        logger.LogInformation("Fetching all dishes");
        var dishes = await db.Dishes.ToListAsync();
        var dishDtos = mapper.Map<List<DishDto>>(dishes);
        return TypedResults.Ok(dishDtos);
    }

    public static async Task<Results<NotFound, Ok<DishDto>>> GetByNameAsync(string name, DishesDbContext db, ClaimsPrincipal user, IMapper mapper, [FromServices] ILogger<DishDto> logger)
    {
        logger.LogInformation("Fetching dish by name: {Name}", name);
        var lowered = name.ToLower();
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Name.ToLower().Contains(lowered));
        if (dish is null)
        {
            logger.LogWarning("Dish not found by name: {Name}", name);
            return TypedResults.NotFound();
        }
        var dishDto = mapper.Map<DishDto>(dish);
        return TypedResults.Ok(dishDto);
    }

    public static async Task<Results<NotFound, Ok<DishDto>>> GetByIdAsync(Guid id, DishesDbContext db, IMapper mapper, [FromServices] ILogger<DishDto> logger)
    {
        logger.LogInformation("Fetching dish by id: {Id}", id);
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Id == id);
        if (dish is null)
        {
            logger.LogWarning("Dish not found by id: {Id}", id);
            return TypedResults.NotFound();
        }
        var dishDto = mapper.Map<DishDto>(dish);
        return TypedResults.Ok(dishDto);
    }

    public static async Task<Results<BadRequest, CreatedAtRoute<DishDto>>> CreateAsync(CreateDishDto dto, DishesDbContext db, IMapper mapper, [FromServices] ILogger<DishDto> logger)
    {
        logger.LogInformation("Creating new dish: {Name}", dto.Name);
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            logger.LogWarning("Attempted to create dish with empty name");
            return TypedResults.BadRequest();
        }
        var dishEntity = mapper.Map<Dish>(dto);
        dishEntity.Id = Guid.NewGuid();
        db.Dishes.Add(dishEntity);
        await db.SaveChangesAsync();
        var createdDto = mapper.Map<DishDto>(dishEntity);
        logger.LogInformation("Dish created with id: {Id}", createdDto.Id);
        return TypedResults.CreatedAtRoute(createdDto, "GetDish", new { id = createdDto.Id });
    }

    public static async Task<Results<NotFound, NoContent>> UpdateAsync(Guid id, UpdateDishDto dto, DishesDbContext db, IMapper mapper, [FromServices] ILogger<DishDto> logger)
    {
        logger.LogInformation("Updating dish with id: {Id}", id);
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Id == id);
        if (dish is null)
        {
            logger.LogWarning("Attempted to update non-existent dish with id: {Id}", id);
            return TypedResults.NotFound();
        }
        mapper.Map(dto, dish);
        await db.SaveChangesAsync();
        logger.LogInformation("Dish updated with id: {Id}", id);
        return TypedResults.NoContent();
    }

    public static async Task<Results<NotFound, NoContent>> DeleteAsync(Guid id, DishesDbContext db, [FromServices] ILogger<DishDto> logger)
    {
        logger.LogInformation("Deleting dish with id: {Id}", id);
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Id == id);
        if (dish is null)
        {
            logger.LogWarning("Attempted to delete non-existent dish with id: {Id}", id);
            return TypedResults.NotFound();
        }
        db.Dishes.Remove(dish);
        await db.SaveChangesAsync();
        logger.LogInformation("Dish deleted with id: {Id}", id);
        return TypedResults.NoContent();
    }
}
