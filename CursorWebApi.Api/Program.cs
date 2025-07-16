using CursorWebApi.Application;
using CursorWebApi.Infrastructure;
using CursorWebApi.Domain;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

builder.Services.AddAuthentication("Beaer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://demo.identityserver.io/"; // Example authority
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// app.MapGet("/products", [Authorize] async (IProductRepository repo) => await repo.GetAllAsync());
// or:
app.MapGet("/products", async (IProductRepository repo) => await repo.GetAllAsync()).RequireAuthorization();


app.MapGet("/products/{id}", async (int id, IProductRepository repo) =>
    await repo.GetByIdAsync(id) is Product product ? Results.Ok(product) : Results.NotFound());

// app.MapPost("/products",  [Authorize(Policy = "AdminOnly")] async (Product product, IProductRepository repo) =>
// {
//     await repo.AddAsync(product);
//     return Results.Created($"/products/{product.Id}", product);
// });
// or:

app.MapPost("/products",  async (Product product, IProductRepository repo) =>
{
    await repo.AddAsync(product);
    return Results.Created($"/products/{product.Id}", product);
}).RequireAuthorization("AdminOnly");



app.MapPut("/products/{id}", async (int id, Product product, IProductRepository repo) =>
{
    product.Id = id;
    await repo.UpdateAsync(product);
    return Results.NoContent();
});

app.MapDelete("/products/{id}", async (int id, IProductRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
});


app.Run();


