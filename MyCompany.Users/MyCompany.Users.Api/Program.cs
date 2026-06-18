using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCompany.Users.API.HealthChecks;
using MyCompany.Users.Application.Interfaces;
using MyCompany.Users.Application.Services;
using MyCompany.Users.Domain.Interfaces;
using MyCompany.Users.Infrastructure.Data;
using MyCompany.Users.Infrastructure.Repositories;
using MyCompany.Users.API.Infrastructure; // 🔹 Dossier local où vous placerez votre nouveau GlobalExceptionHandler
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 💻 CONFIGURATION DE L'HÔTE & KESTREL
// ==========================================
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = null;
    });
});

// ==========================================
// 1️⃣ SERVICES DE BASE & INFRASTRUCTURE
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// 🔹 GESTION DES EXCEPTIONS MODERNE (.NET 8)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ==========================================
// 2️⃣ DOCUMENTATION SWAGGER (JWT SUPPORT)
// ==========================================
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
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================
// 3️⃣ SÉCURITÉ : AUTHENTIFICATION & CORS
// ==========================================
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7282") // Port de votre passerelle/front
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ==========================================
// 4️⃣ DONNÉES & PERSISTANCE (EF CORE)
// ==========================================
builder.Services.AddDbContext<AppDbContext>(options => // 🔹 REMPLACÉ PAR AppDbContext
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("MyCompany.Users.Infrastructure")));

// ==========================================
// 5️⃣ MESSAGING (MASSTRANSIT + OUTBOX PATTERN)
// ==========================================
// 🔹 REMPLACÉ PAR AppDbContext ICI AUSSI
builder.Services.AddMassTransit(x =>
{
    // 🔹 L'erreur peut venir d'ici si UseSqlServer() ou UseBusOutbox() est manquant !
    x.AddEntityFrameworkOutbox<AppDbContext>(o =>
    {
        o.UseSqlServer(); // Dit à l'outbox d'utiliser SQL Server
        o.UseBusOutbox(); // Active la publication des messages via l'outbox
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // 🔹 TRÈS IMPORTANT : Configure l'Outbox sur le bus global
        cfg.ConfigureEndpoints(context);
    });
});

// ==========================================
// 6️⃣ INJECTION DE DÉPENDANCES LOGIQUES
// ==========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
// 💡 Note : RabbitMqPublisher a été supprimé car MassTransit (IPublishEndpoint) le remplace avantageusement.

// ==========================================
// 7️⃣ DIAGNOSTICS & HEALTHCHECKS
// ==========================================
builder.Services.AddHealthChecks()
    .AddCheck<CacheHealthCheck>("Cache", tags: new[] { "cache" })
    .AddCheck<DatabaseHealthCheck>("Database", tags: new[] { "database" });

var app = builder.Build();

// ==========================================
// 🧅 PIPELINE DE MIDDLEWARES (L'ORDRE EST CRUCIAL)
// ==========================================

// 🔹 1. Gestion globale des erreurs au tout début du cycle
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// 2. Sécurité et Routage de base
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 3. Authentification & Autorisation (Obligatoirement AVANT les contrôleurs)
app.UseAuthentication();
app.UseAuthorization();

// 4. Contrôleurs de l'API
app.MapControllers();

// 5. Endpoints de Monitoring (HealthChecks)
app.MapHealthChecks("/health/cache", new HealthCheckOptions { Predicate = c => c.Tags.Contains("cache"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("/health/database", new HealthCheckOptions { Predicate = c => c.Tags.Contains("database"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

app.Run();