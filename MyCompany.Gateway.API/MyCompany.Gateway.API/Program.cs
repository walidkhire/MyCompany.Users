using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyCompany.Gateway.API.Middlewares;
using Prometheus;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. On enregistre le handler standard
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


// 🔹 Services de base
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

// 🔹 Configuration Swagger avec Support JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyCompany API Gateway",
        Version = "v1",
        Description = "Gateway YARP pour Users & Orders"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Authorization: Bearer {token}",
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

// 🔹 YARP Reverse Proxy
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 🔹 Authentification JWT (Centralisée sur la Gateway)
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();



// Etape 1 🔹 Configuration des CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("MonFrontendPolicy", policy =>
//    {
//        policy.WithOrigins("http://localhost:7282", "http://localhost:3000") // 🔹 Remplacez par les URLs exactes de vos Frontends (Angular, React...)
//              .AllowAnyMethod()  // Autorise GET, POST, PUT, DELETE
//              .AllowAnyHeader()  // Autorise les en-têtes personnalisés (dont l'en-tête Authorization pour votre JWT !)
//              .AllowCredentials(); // Autorise les cookies de session si nécessaire
//    });
//});


builder.Services.AddCors(options =>
{
    options.AddPolicy("MonFrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin() // En démo locale, on peut autoriser tout
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 2. On utilise l'extension native de .NET 8 (qui remplace votre UseGlobalExceptionHandling)
app.UseExceptionHandler();


// 🔹 Pipeline de Middlewares
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// 🔹 ÉTAPE 2 : Activer le middleware CORS (Obligatoirement AVANT MapReverseProxy)
app.UseCors("MonFrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

// 🔹 Observabilité et Métriques (Prometheus & HealthChecks)
app.UseHttpMetrics();
app.MapMetrics();
app.MapHealthChecks("/health");

app.MapControllers();

// 🔹 Activation du Reverse Proxy (Doit rester à la fin)
app.MapReverseProxy();

app.Run();