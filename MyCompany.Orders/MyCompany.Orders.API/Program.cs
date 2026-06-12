using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCompany.Orders.API.Consumers;
using MyCompany.Orders.Application.Interfaces;
using MyCompany.Orders.Application.Services;
using MyCompany.Orders.Domain.Interfaces;
using MyCompany.Orders.Infrastructure.Data;
using MyCompany.Orders.Infrastructure.HttpClients;
using MyCompany.Orders.Infrastructure.Repositories;
using MyCompany.Shared.Middlewares;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Services de base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2️⃣ Configuration Swagger avec Support JWT
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

// 3️⃣ Base de données SQL Server (Ciblée sur le projet Infrastructure)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString, x =>
        x.MigrationsAssembly("MyCompany.Orders.Infrastructure")));

// 4️⃣ Communication Événementielle (MassTransit + RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    // Enregistrement du consommateur d'événements provenant de Users
    x.AddConsumer<UserCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Liaison automatique de la file d'attente
        cfg.ReceiveEndpoint("user-created-queue", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });
    });
});

// 5️⃣ Injection de Dépendances (DI) - Alignée à la même version d'EF Core
builder.Services.AddHttpClient<IUsersClient, UsersClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7070/");
});
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHttpContextAccessor();

// 6️⃣ Validation locale du Token JWT (Parfaitement synchronisée)
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

// 7️⃣ Pipeline des Middlewares (Ordonné de la même façon que Users)
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

// Authentification & Autorisation obligatoires avant l'accès aux routes
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();