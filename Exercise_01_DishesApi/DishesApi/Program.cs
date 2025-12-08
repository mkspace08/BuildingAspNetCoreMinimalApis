using DishesApi.DbContexts;
using Microsoft.EntityFrameworkCore;
using DishesApi.EndpointBuilders;
using FluentValidation;
using DishesApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// register the DbContext on the container, getting the
// connection string from appSettings   
builder.Services.AddDbContext<DishesDbContext>(o => o.UseSqlite(
    builder.Configuration["ConnectionStrings:DishesDBConnectionString"]));
// Register AutoMapper and scan all assemblies for profiles
// This enables object-object mapping for DTOs and entities
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateDishDtoValidator>();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler(); // Logs exceptions globally
//}
//else
//{
//    app.UseDeveloperExceptionPage();
//}

if (!app.Environment.IsDevelopment())
{

    //app.UseExceptionHandler();
    //app.UseExceptionHandler(configureApplicationBuilder =>
    //{
    //    configureApplicationBuilder.Run(async context =>
    //    {
    //        context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
    //        context.Response.ContentType = "text/html";
    //        await context.Response.WriteAsync("An unexpected problem happened.");
    //    });
    //});

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }
}

app.UseHttpsRedirection();

// Register endpoints directly
app.RegisterDishesEndpoints();
app.MapIngredientsEndpoints();
app.MapErrorsEndpoint();

// recreate & migrate the database on each run, for demo purposes
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}

app.Run();

