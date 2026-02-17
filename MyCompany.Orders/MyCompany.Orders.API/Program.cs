using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyCompany.Orders.API.Consumers;
using MyCompany.Orders.Application.Interfaces;
using MyCompany.Orders.Application.Services;
using MyCompany.Orders.Domain.Interfaces;
using MyCompany.Orders.Infrastructure.Data;
using MyCompany.Orders.Infrastructure.HttpClients;
using MyCompany.Orders.Infrastructure.Repositories;
using MyCompany.Shared.Middlewares;


var builder = WebApplication.CreateBuilder(args);

// ==========================
// 1️⃣ Controllers + Swagger
// ==========================
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

// ==========================
// 3️⃣ DB SQL Server (LocalDB)
// ==========================

// Modifiez votre section 3️⃣ comme ceci :
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(connectionString, x =>
        x.MigrationsAssembly("MyCompany.Orders.Infrastructure"))); // 👈 On force l'assemblage ici
                                                                   //
//C: \Users\walid\source\repos\MyCompany.Users\MyCompany.Orders\MyCompany.Orders.API>dotnet ef migrations add InitialCreate --project ../MyCompany.Orders.Infrastructure/MyCompany.Orders.Infrastructure.csproj --startup-project MyCompany.Orders.API.csproj
 

// ==========================
// 4 MassTransit + RabbitMQ
// ==========================
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("user-created-queue", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });
    });
});


// ==========================
// 5. DB InMemory pour tests
// ==========================
builder.Services.AddDbContext<OrdersDbContext>(o =>
    o.UseInMemoryDatabase("OrdersDb"));

// ==========================
// 4️⃣ Dependency Injection
// ==========================
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHttpContextAccessor(); // ✅ obligatoire pour UsersClient
builder.Services.AddHttpClient<IUsersClient, UsersClient>(c =>
{
    c.BaseAddress = new Uri("https://localhost:7070"); // Users.API
});

// ==========================
// 5️⃣ JWT Authentication
// ==========================
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:7070"; // Users.API JWT
        options.Audience = "MyCompanyUsers";
        options.RequireHttpsMetadata = false; // dev only
    });

builder.Services.AddAuthorization();

// ==========================
// 6️⃣ Build app
// ==========================
var app = builder.Build();

// ==========================
// 7️⃣ Middleware
// ==========================
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ==========================
// 8️⃣ Swagger
// ==========================
app.UseSwagger();
app.UseSwaggerUI();

// ==========================
// 9️⃣ Map controllers
// ==========================
app.MapControllers();

// ==========================
//  🔹 Start MassTransit
// ==========================
app.Run();
