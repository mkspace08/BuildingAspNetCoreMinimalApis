using DishesAPI.DbContexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// register the DbContext on the container, getting the
// connection string from appSettings   
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/dishes", async (DishesDbContext dishesDbContext) =>
{
    return dishesDbContext.Dishes.ToListAsync();
});

// Get first dish that matches the provided name (case-insensitive, substring match)
app.MapGet("/dishes/{name}", async (string name, DishesDbContext dishesDbContext) =>
{
    var lowered = name.ToLower();
    var dish = await dishesDbContext.Dishes
        .FirstOrDefaultAsync(d => d.Name.ToLower().Contains(lowered));

    return dish is not null ? Results.Ok(dish) : Results.NotFound();
});

app.MapGet("/dishes/{id:guid}", async (Guid id, DishesDbContext dishesDbContext) =>
{
    var dish = await dishesDbContext.Dishes
        //.Include(d => d.Ingredients)
        .FirstOrDefaultAsync(d => d.Id == id);

    return dish is not null ? Results.Ok(dish) : Results.NotFound();
});

app.MapGet("/dishes/{dishId}/ingredients", async (Guid dishId, DishesDbContext dishesDbContext) =>
{
    var dish = await dishesDbContext.Dishes
        .Include(d => d.Ingredients)
        .FirstOrDefaultAsync(d => d.Id == dishId);
    if (dish is null)
        return Results.NotFound();
    return Results.Ok(dish.Ingredients);
});

// recreate & migrate the database on each run, for demo purposes
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}

app.Run();

