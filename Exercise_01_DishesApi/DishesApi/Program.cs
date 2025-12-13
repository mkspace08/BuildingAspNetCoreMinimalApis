using DishesApi.DbContexts;
using Microsoft.EntityFrameworkCore;
using DishesApi.EndpointBuilders;
using FluentValidation;
using DishesApi.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

// Configure JWT authentication using settings from appsettings.json


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminFromBelgium", policy =>
        policy
            .RequireRole("admin")
            .RequireClaim("country", "Belgium"));

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

app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"Authorization header: {authHeader}");
        // If you want to log just the token part:
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
           // Console.WriteLine($"JWT token: {token}");
        }
    }
    await next();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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

var signingKeys = builder.Configuration.GetSection("Authentication:Schemes:Bearer:SigningKeys").GetChildren();
foreach (var key in signingKeys)
{
    var value = key["Value"];
    var issuer = key["Issuer"];
    Console.WriteLine($"Signing key: {value}, Issuer: {issuer}");
}

app.Run();

