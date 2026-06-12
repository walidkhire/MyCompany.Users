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
using Users.API.Services; // 👈 AJOUTÉ : Pour que le type RabbitMqPublisher soit reconnu

var builder = WebApplication.CreateBuilder(args);

// Configuration HTTPS Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = null;
    });
});

// 1️⃣ Services de base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2️⃣ Configuration Swagger avec Support JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyCompany Users API", Version = "v1" });
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

// 3️⃣ Authentification JWT Locale
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

// 4️⃣ Base de données SQL Server (Ciblée sur le projet Infrastructure)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("MyCompany.Users.Infrastructure")));

// 5️⃣ Communication Événementielle (MassTransit / Producteur RabbitMQ)
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

// 6️⃣ Injection de Dépendances & Cache
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// 🔹 CORRIGÉ : Enregistrement du Publisher en Singleton pour le controlleur
builder.Services.AddSingleton<RabbitMqPublisher>();

// 7️⃣ Configuration des CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7282")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 8️⃣ Diagnostics & HealthChecks
builder.Services.AddHealthChecks()
    .AddCheck<CacheHealthCheck>("Cache", tags: new[] { "cache" })
    .AddCheck<DatabaseHealthCheck>("Database", tags: new[] { "database" });
   // .AddCheck<GaiaVeApiHealthCheck>("Api", tags: new[] { "api" });

var app = builder.Build();

// 1️⃣0️⃣ Pipeline de Middlewares ordonné stratégiquement
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(a => a.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature != null)
        {
            var ex = feature.Error;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
    }));
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Middlewares d'infrastructure techniques (Log & Exception)
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Authentification & Autorisation d'abord
app.UseAuthentication();
app.UseAuthorization();

// Validation métier : S'exécute uniquement si la requête est authentifiée
app.UseMiddleware<ValidationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1️⃣2️⃣ Endpoints de monitoring et HealthChecks
app.MapHealthChecks("/health/cache", new HealthCheckOptions { Predicate = c => c.Tags.Contains("cache"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("/health/database", new HealthCheckOptions { Predicate = c => c.Tags.Contains("database"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("/health/api", new HealthCheckOptions { Predicate = c => c.Tags.Contains("api"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

app.MapControllers();

app.Run();