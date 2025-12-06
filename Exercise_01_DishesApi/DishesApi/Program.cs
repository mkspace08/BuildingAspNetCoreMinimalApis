using AutoMapper;
using DishesApi.DbContexts;
using DishesApi.Models;
using DishesApi.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// register the DbContext on the container, getting the
// connection string from appSettings   
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

// Register AutoMapper and scan all assemblies for profiles
// This enables object-object mapping for DTOs and entities
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Group all /dishes endpoints
var dishes = app.MapGroup("/dishes");

dishes.MapGet("", async Task<Ok<List<DishDto>>> (DishesDbContext dishesDbContext, IMapper mapper) =>
{
    var dishes = await dishesDbContext.Dishes.ToListAsync();
    var dishDtos = mapper.Map<List<DishDto>>(dishes);
    return TypedResults.Ok(dishDtos);
});

// Get first dish that matches the provided name (case-insensitive, substring match)
dishes.MapGet("/{name}", async Task<Results<NotFound, Ok<DishDto>>> (string name, DishesDbContext dishesDbContext, ClaimsPrincipal claimsPrincipal, IMapper mapper) =>
{
    Console.WriteLine($"User: {claimsPrincipal.Identity?.Name ?? "anonymous"} is searching for dish with name containing: {name}, Is authenticated: {claimsPrincipal.Identity?.IsAuthenticated}");

    var lowered = name.ToLower();
    var dish = await dishesDbContext.Dishes
        .FirstOrDefaultAsync(d => d.Name.ToLower().Contains(lowered));
    if (dish is null)
        return TypedResults.NotFound();
    var dishDto = mapper.Map<DishDto>(dish);
    return TypedResults.Ok(dishDto);
});

dishes.MapGet("/{id:guid}", async Task<Results<NotFound, Ok<DishDto>>> (Guid id, DishesDbContext dishesDbContext, IMapper mapper) =>
{
    var dish = await dishesDbContext.Dishes
        //.Include(d => d.Ingredients)
        .FirstOrDefaultAsync(d => d.Id == id);
    if (dish is null)
        return TypedResults.NotFound();
    var dishDto = mapper.Map<DishDto>(dish);
    return TypedResults.Ok(dishDto);
})
.WithName("GetDish");

dishes.MapGet("/{dishId}/ingredients", async Task<Results<NotFound, Ok<List<IngredientDto>>>> (Guid dishId, DishesDbContext dishesDbContext, IMapper mapper) =>
{
    var dish = await dishesDbContext.Dishes
        .Include(d => d.Ingredients)
        .FirstOrDefaultAsync(d => d.Id == dishId);
    if (dish is null)
        return TypedResults.NotFound();
    var ingredientDtos = mapper.Map<List<IngredientDto>>(dish.Ingredients);
    return TypedResults.Ok(ingredientDtos);
});

dishes.MapPost("", async Task<Results<BadRequest, CreatedAtRoute<DishDto>>> (
    CreateDishDto createDishDto,
    DishesDbContext dishesDbContext,
    IMapper mapper) =>
{
    if (string.IsNullOrWhiteSpace(createDishDto.Name))
        return TypedResults.BadRequest();

    var dishEntity = mapper.Map<Dish>(createDishDto);
    dishEntity.Id = Guid.NewGuid();
    dishesDbContext.Dishes.Add(dishEntity);
    await dishesDbContext.SaveChangesAsync();
    var createdDto = mapper.Map<DishDto>(dishEntity);
    return TypedResults.CreatedAtRoute(createdDto, "GetDish", new { id = createdDto.Id });
});

dishes.MapPut("/{id:guid}", async Task<Results<NotFound, NoContent>> (
    Guid id,
    UpdateDishDto updateDishDto,
    DishesDbContext dishesDbContext,
    IMapper mapper) =>
{
    var dish = await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == id);
    if (dish is null)
        return TypedResults.NotFound();

    // Map updated fields from DTO to entity
    mapper.Map(updateDishDto, dish);
    await dishesDbContext.SaveChangesAsync();
    return TypedResults.NoContent();
});

dishes.MapDelete("/{id:guid}", async Task<Results<NotFound, NoContent>> (
    Guid id,
    DishesDbContext dishesDbContext) =>
{
    var dish = await dishesDbContext.Dishes.FirstOrDefaultAsync(d => d.Id == id);
    if (dish is null)
        return TypedResults.NotFound();

    dishesDbContext.Dishes.Remove(dish);
    await dishesDbContext.SaveChangesAsync();
    return TypedResults.NoContent();
});

// recreate & migrate the database on each run, for demo purposes
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}

app.Run();

