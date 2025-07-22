My requirement:
let create .net core web api using: jwt, openapi, cors,  for a CRUD related process, using Authentication & Authorization(jwt) secured project, role and policy based). CORS, OpenAPI, caching using sql server, rate limiting and throttling, log serilog, centralizd loogging, Exception handling, filter(Authorization Filters, Action Filters,Result Filters,Exception Filters,Resource Filters.,Custom Middleware, routing(Attribute Routing,, Route Prefixes, Convention-Based Routing)

I want you using best architecture, pattern to do this.
can you do this? I want you add one function by add comments.
I want you step by step to finishe these fucntion.

basically, I want you get all aspects of a webapi involved in, but with simple business logic (but if needed, add a little more logic is ok)

for exception, I want you combine different exception handler including some centralized.

******************************************
cursor plan:
Absolutely! Your requirements are to build a .NET Core Web API with the following features, using best practices and architecture:
Features to include:
JWT Authentication & Authorization (role and policy-based)
CORS
OpenAPI (Swagger)
CRUD operations (with simple business logic)
SQL Server (with caching)
Rate limiting & throttling
Serilog for centralized logging
Exception handling
Filters (Authorization, Action, Result, Exception, Resource)
Custom Middleware
Routing (Attribute, Route Prefixes, Convention-Based)
Best architecture/patterns:
We'll use Clean Architecture (or Onion/Hexagonal), Dependency Injection, Repository Pattern, and separation of concerns.
Step-by-step plan:
Project Setup
Create a new .NET Core Web API project.
Set up solution structure (API, Application, Domain, Infrastructure).
Add CRUD Entity
Define a simple entity (e.g., Product).
Implement Repository and Service layers.
Add basic CRUD endpoints.
Authentication & Authorization
Add JWT authentication.
Implement role and policy-based authorization.
CORS
Configure CORS for the API.
OpenAPI (Swagger)
Add and configure Swagger.
SQL Server & Caching
Set up EF Core with SQL Server.
Add caching (e.g., in-memory or distributed).
Rate Limiting & Throttling
Add rate limiting middleware.
Serilog & Centralized Logging
Integrate Serilog for logging.

Add global exception handling middleware.
Filters
Implement and demonstrate all filter types.
Custom Middleware
Add a sample custom middleware.
Routing
Demonstrate attribute routing, route prefixes, and convention-based routing.

Exception Handling:
1. Centralized Exception Handling Middleware
This is a global handler that catches all unhandled exceptions and returns a consistent error response.
It’s the main line of defense and is recommended for most APIs.
2. Exception Filters
These are part of the ASP.NET Core filter pipeline.
They allow you to handle exceptions at the controller/action level or globally (registered in Startup).
Useful for handling exceptions in a more granular way (e.g., for specific controllers or actions).
3. Try-Catch in Business Logic
For very specific cases, you might want to catch and handle exceptions inside your service or controller methods.
This is for cases where you want to recover or handle certain exceptions differently.

Centralized Middleware: Handles all unhandled exceptions and logs them.
Exception Filters: Used for custom exception types or to override the global handler for specific scenarios.
Try-Catch: Only for very specific, recoverable exceptions in business logic.


******************************************
real steps:

1. run cli to create sub projects, and add references.
dotnet new sln -n CursorWebApi
dotnet new webapi -n CursorWebApi.Api
dotnet new classlib -n CursorWebApi.Application
dotnet new classlib -n CursorWebApi.Domain
dotnet new classlib -n CursorWebApi.Infrastructure

dotnet sln add CursorWebApi.Api/CursorWebApi.Api.csproj
dotnet sln add CursorWebApi.Application/CursorWebApi.Application.csproj
dotnet sln add CursorWebApi.Domain/CursorWebApi.Domain.csproj
dotnet sln add CursorWebApi.Infrastructure/CursorWebApi.Infrastructure.csproj

Step 1.1: Create the Solution and Projects
dotnet new sln -n CursorWebApi
dotnet new webapi -n CursorWebApi.Api
dotnet new classlib -n CursorWebApi.Application
dotnet new classlib -n CursorWebApi.Domain
dotnet new classlib -n CursorWebApi.Infrastructure

dotnet sln add CursorWebApi.Api/CursorWebApi.Api.csproj
dotnet sln add CursorWebApi.Application/CursorWebApi.Application.csproj
dotnet sln add CursorWebApi.Domain/CursorWebApi.Domain.csproj
dotnet sln add CursorWebApi.Infrastructure/CursorWebApi.Infrastructure.csproj

dotnet add CursorWebApi.Application package Microsoft.Extensions.Logging.Abstractions

dotnet run

 taskkill /F /IM dotnet.exe   ---- for to stop other processes taken

add reference:
dotnet add CursorWebApi.Infrastructure reference CursorWebApi.Application

build:
dotnet build --no-incremental


add jwt:
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

dotnet add package Microsoft.AspNetCore.RateLimiting // not working though

dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File


dotnet add CursorWebApi.Api package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add CursorWebApi.Api package Microsoft.Extensions.Caching.Abstractions

docker run -d --name redis-test -p 6379:6379 redis



Exception Handling Plan:
Global Exception Middleware - Catch all unhandled exceptions
Custom Exception Types - Business logic exceptions
Exception Filters - Handle specific exception types
Consistent Error Responses - Standardized error format
Logging Integration - Log exceptions for debugging



done:
Project Setup: Solution and project structure (API, Application, Domain, Infrastructure)
CRUD Entity: Product entity, repository, service, and CRUD endpoints (minimal API)
Authentication & Authorization: JWT, role-based policies
CORS: Configured
OpenAPI (Swagger): Configured and working
Exception Handling: Global middleware, endpoint filters

next:
1. SQL Server & Caching
Set up EF Core with SQL Server
Add caching (in-memory or distributed)
2. Rate Limiting & Throttling
Add middleware for rate limiting
3. Serilog & Centralized Logging
Integrate Serilog for structured logging
4. Filters & Middleware
Demonstrate custom middleware and endpoint filters
(You’ve already done some of this with exception handling!)
5. Routing
Attribute routing, route prefixes, convention-based routing (mainly for controllers, but can show minimal API patterns)


why sometimes we says put the except handler at the front of pipeline?
