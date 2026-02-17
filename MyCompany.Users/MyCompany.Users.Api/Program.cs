using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCompany.Shared.Middlewares;
using MyCompany.Users.API.HealthChecks;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Users.Application.Services;
using MyCompany.Users.Domain.Interfaces;
using MyCompany.Users.Infrastructure.Data;
using MyCompany.Users.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 🔹 HTTPS (dotnet dev-certs)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = null;
    });
});

// ======================
// 1️⃣ Controllers
// ======================
builder.Services.AddControllers();

// ======================
// 2️⃣ Swagger + JWT
// ======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Entrez : Bearer {votre_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ======================
// 3️⃣ JWT Authentication
// ======================
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ======================
// 4️⃣ Database
// ======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("MyCompany.Users.API")
    ));

// ======================
// 5️⃣ MassTransit (RabbitMQ)
// ======================
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

// ======================
// 6️⃣ Dependency Injection
// ======================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient(); // pour API clients si besoin

// ======================
// 7️⃣ CORS
// ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7282")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ======================
// 8️⃣ HealthChecks
// ======================
builder.Services.AddHealthChecks()
    .AddCheck<CacheHealthCheck>("Cache", tags: new[] { "cache" })
    .AddCheck<DatabaseHealthCheck>("Database", tags: new[] { "database" })
    .AddCheck<GaiaVeApiHealthCheck>("Api", tags: new[] { "api" });

// ======================
// 9️⃣ Build
// ======================
var app = builder.Build();

// ======================
// 10️⃣ Middleware global
// ======================

// Exception handling pour prod
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(a => a.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature != null)
        {
            var ex = feature.Error;
            var problem = new
            {
                error = ex.Message,
                stackTrace = app.Environment.IsDevelopment() ? ex.StackTrace : null
            };
            await context.Response.WriteAsJsonAsync(problem);
        }
    }));
}

// Redirection HTTPS
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowFrontend");

// Middlewares personnalisés
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<ValidationMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ======================
// 11️⃣ Swagger
// ======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ======================
// 12️⃣ HealthCheck endpoints
// ======================
app.MapHealthChecks("/health/cache", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("cache"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/database", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/api", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// ======================
// 13️⃣ Controllers
// ======================
app.MapControllers();

// ======================
// 14️⃣ Run
// ======================
app.Run();
