using DishesApi.DbContexts;
using DishesApi.EndpointBuilders;
using DishesApi.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

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

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("MyBearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    //It has differences in older versions like 6.5 of Microsoft.AspNetCore.OpenApi package
    //Maybe there should be used older version of it.
    options.AddSecurityRequirement(openApiDocument => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("MyBearer", openApiDocument)
            {
                Description = "JWT Authorization header using the Bear",
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
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

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

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

