using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCompany.Orders.API.Consumers; 
using MyCompany.Orders.API.Middlewares;
using MyCompany.Orders.Application.Interfaces;
using MyCompany.Orders.Application.Services;
using MyCompany.Orders.Application.Validators;
using MyCompany.Orders.Domain.Interfaces;
using MyCompany.Orders.Infrastructure.Data;
using MyCompany.Orders.Infrastructure.HttpClients;
using MyCompany.Orders.Infrastructure.Repositories;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1️⃣ GESTION DES EXCEPTIONS & VALIDATION
// ==========================================
// 🔹 Configuration native .NET 8 (Remplace définitivement l'ancien middleware)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

// ==========================================
// 2️⃣ SERVICES DE BASE & SWAGGER (JWT)
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyCompany Orders API", Version = "v1" });
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
// 3️⃣ BASE DE DONNÉES (EF CORE)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString, x =>
        x.MigrationsAssembly("MyCompany.Orders.Infrastructure")));

// ==========================================
// 4️⃣ MESSAGING (MASSTRANSIT + RABBITMQ + INBOX)
// ==========================================
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

    // 🔹 CONFIGURATION SENIOR : Ajout de l'Inbox/Outbox pour l'idempotence des messages reçus
    x.AddEntityFrameworkOutbox<OrdersDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("user-created-queue", e =>
        {
            // 🔹 Active l'inbox transactionnelle sur ce consommateur
            e.UseEntityFrameworkOutbox<OrdersDbContext>(context);
            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });
    });
});

// ==========================================
// 5️⃣ INJECTION DE DÉPENDANCES & RESILIENCE HTTP
// ==========================================
builder.Services.AddHttpClient<IUsersClient, UsersClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7070/");
})
.AddStandardResilienceHandler(options =>
{
    // 1️⃣ Timeout d'une tentative : On le descend à 3 secondes (au lieu de 10s par défaut)
    // L'application n'attendra pas indéfiniment si Users.API est bloqué.
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);

    // 2️⃣ Circuit Breaker : On analyse les échecs sur une fenêtre de 30 secondes.
    // 30s est bien supérieur au double de 3s (6s), respectant ainsi la règle stricte.
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);

    // Reste de vos configurations excellentes :
    options.CircuitBreaker.FailureRatio = 0.5; // Déclenche si 50% des requêtes échouent
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30); // Circuit ouvert pendant 30s
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHttpContextAccessor();

// ==========================================
// 6️⃣ SECURITÉ : AUTHENTIFICATION JWT
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

var app = builder.Build();

// ==========================================
// 🧅 PIPELINE DE MIDDLEWARES HTTP
// ==========================================

// 🔹 1. Capture immédiate de toutes les erreurs du pipeline
app.UseExceptionHandler();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 2. Securité (Toujours avant le mappage des contrôleurs)
app.UseAuthentication();
app.UseAuthorization();

// 🔹 3. Exposition des Endpoints métiers
app.MapControllers();

app.Run();