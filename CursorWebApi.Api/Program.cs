using CursorWebApi.Application;
using CursorWebApi.Infrastructure;
using CursorWebApi.Domain;
using Microsoft.Extensions.Options;
using CursorWebApi.Api.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using CursorWebApi.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
// using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using Serilog;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;

// Configure Serilog at the very top (before builder)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/webapi.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
// register services

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>

{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// when we do below register, it applies to all controllers and actions;
// if an exception (e.g., ValidationException) is thrown in an action method and is not caught by a try-catch block,
// the filter’s OnException method will be called.
// If the filter handles the exception (by setting context.ExceptionHandled = true;),
// the exception will not propagate further (e.g., to your global middleware).
// If the filter handles the exception (by setting context.ExceptionHandled = true;),
// the exception will not propagate further (e.g., to your global middleware).
// this for MVC register
// builder.Services.AddControllers(options =>
// {
//     options.Filters.Add<CursorWebApi.Api.Filters.CustomExceptionFilter>();
// });

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Use Singleton for in-memory repository to persist data across requests
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"];
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated successfully");
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
                Console.WriteLine($"Claims: {string.Join(", ", claims ?? Array.Empty<string>())}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($"📨 Message received. Token: {context.Token?.Substring(0, Math.Min(20, context.Token?.Length ?? 0))}...");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));

// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
// });

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// builder.Services.AddRateLimiter(options =>
// {
//     options.AddFixedWindowLimiter("fixed", limiterOptions =>
//     {
//         limiterOptions.PermitLimit = 5; // 5 requests
//         limiterOptions.Window = TimeSpan.FromSeconds(10); // per 10 seconds
//         limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//         limiterOptions.QueueLimit = 2; // 2 extra requests can queue
//     });
// });

// Configure in-memory caching
builder.Services.AddMemoryCache(options =>
{
    // options.SizeLimit = 1024; // Maximum number of cache entries
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5); // How often to scan for expired entries
});
   builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
   builder.Services.AddInMemoryRateLimiting();
   builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Change to your Redis server
    options.InstanceName = "SampleInstance";
});





var app = builder.Build();

// app.UseExceptionHandler(errorApp =>
// {
//     errorApp.Run(async context =>
//     {
//         var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
//         var exception = exceptionHandlerPathFeature?.Error;

//         // Custom logic: log, inspect request, etc.
//         if (exception != null)
//         {
//             // Example: circuit-break and return custom response
//             context.Response.StatusCode = 500;
//             context.Response.ContentType = "application/json";
//             await context.Response.WriteAsync(JsonSerializer.Serialize(new
//             {
//                 message = "Custom error handler caught this: " + exception.Message
//             }));
//         }
//     });
// });

// configure middlewares
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<CursorWebApi.Api.Middleware.RequestTimingMiddleware>();
app.UseMiddleware<CursorWebApi.Api.Middleware.CustomHeaderMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>();

// globally use or per endpoint (see below)
// app.UseRateLimiter();
app.UseIpRateLimiting();

// map endpoints

// app.UseExceptionHandler("/error");

// app.Map("/error", (HttpContext context) =>
// {
//     var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
//     var exception = exceptionHandlerPathFeature?.Error;
//     return Results.Problem(
//         detail: exception?.Message,
//         title: "A custom error occurred"
//     );
// });

app.MapGet("/show-request-header", (HttpContext context, ILogger<Program> logger) =>
{
    var received = context.Request.Headers["X-Request-Received"].FirstOrDefault();
    logger.LogInformation("X-Request-Received header value: {Received}", received);
    return Results.Ok(new { ReceivedAt = received ?? "Not set" });
});

// Cache monitoring endpoint
app.MapGet("/cache/stats", (IMemoryCache cache) =>
{
    // Note: IMemoryCache doesn't expose statistics directly
    // In a real application, you'd use a more sophisticated cache implementation
    return Results.Ok(new {
        Message = "Cache is active",
        Note = "Use logging to monitor cache hits/misses",
        CacheType = "In-Memory Cache"
    });
});

    app.MapGet("/redis-test", async (IDistributedCache cache) =>
    {
        var key = "test-key";
        var value = await cache.GetStringAsync(key);
        if (value == null)
        {
            value = $"Set at {DateTime.Now}";
            await cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });
            return $"Cache miss. Setting value: {value}";
        }
        return $"Cache hit! Value: {value}";
    });

app.MapGet("/test-auth", () => "✅ Authentication working!").RequireAuthorization();
// app.MapGet("/products", [Authorize] async (IProductRepository repo) => await repo.GetAllAsync());
// or:
app.MapGet("/products", async (IProductService service) =>
        await service.GetAllProductsAsync());
        //.RequireAuthorization();


app.MapGet("/products/{id}", async (int id, IProductService service) =>
    await service.GetProductByIdAsync(id) is Product product ? Results.Ok(product) : Results.NotFound());

//app.MapPost("/products", async (string name, decimal price, string category, int stockQuantity, IProductService service) =>
//{
//    var product = await service.CreateProductAsync(name, price, category, stockQuantity);
//    return Results.Created($"/products/{product.Id}", product);
//})
////.RequireAuthorization("AdminOnly")
//// below 2 use only 1, otherwise the second will make the first never be triggered if there is any error.
//.AddEndpointFilter<CursorWebApi.Api.Filters.ExceptionEndpointFilter>()
//.AddEndpointFilter<CursorWebApi.Api.Filters.ValidationAndLoggingEndpointFilter>()


//.RequireRateLimiting("fixed");  // endpoint level, also can globally, see above

app.MapPut("/products/{id}", async (int id, string name, decimal price, string category, IProductService service) =>
{
    await service.UpdateProductAsync(id, name, price, category);
    return Results.NoContent();
});

app.MapDelete("/products/{id}", async (int id, IProductService service) =>
{
    await service.DeleteProductAsync(id);
    return Results.NoContent();
});

// Business rule endpoints
app.MapGet("/products/category/{category}", async (string category, IProductService service) =>
    await service.GetProductsByCategoryAsync(category));

app.MapGet("/products/low-stock", async (int threshold, IProductService service) =>
    await service.GetLowStockProductsAsync(threshold));

app.MapGet("/products/{id}/discount", async (int id, decimal discountPercentage, IProductService service) =>
{
    var discountedPrice = await service.CalculateProductDiscountAsync(id, discountPercentage);
    return Results.Ok(new { DiscountedPrice = discountedPrice, DiscountPercentage = discountPercentage });
});


app.MapPost("/login", (UserLogin login) =>
{
    // For demo: hardcoded user. Replace with real user validation.
    if (login.Username == "admin" && login.Password == "password")
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login.Username),
            new Claim(ClaimTypes.Role, "admin")
        };

        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new { token = tokenString });
    }

    return Results.Unauthorized();
});

// Example: Logging in a minimal API endpoint
app.MapGet("/logtest", (ILogger<Program> logger) =>
{
    logger.LogInformation("This is an info log from /logtest endpoint!");
    logger.LogWarning("This is a warning log!");
    logger.LogError("This is an error log!");
    return Results.Ok("Logged some messages! Check the console and logs/webapi.log");
});

app.Run();





