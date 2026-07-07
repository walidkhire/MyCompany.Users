using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MyCompany.Frontend.Web.Services;
using MyCompany.Frontend.Web.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// 1. Enregistrement des services applicatifs de base
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ToastService>();

// 2. Enregistrement du Handler d'interception des requêtes HTTP
builder.Services.AddTransient<JwtHttpMessageHandler>();

// 3. CONFIGURATION DU HTTPCLIENT DÉDIÉ À LA GATEWAY API
builder.Services.AddHttpClient("GatewayClient", (sp, client) =>
{
    var gatewayAddress = builder.Configuration["GatewayAddress"] ?? "http://localhost:5280";
    client.BaseAddress = new Uri(gatewayAddress);
})
.AddHttpMessageHandler<JwtHttpMessageHandler>();

// 🟢 LA CLÉ : Le HttpClient par défaut DOIT pointer sur le HostEnvironment pour que Blazor respire
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 🟢 On enregistre l'instance de la Gateway sous un nom distinct ou via une factory pour nos composants
builder.Services.AddScoped(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return httpClientFactory.CreateClient("GatewayClient");
});

// 4. Configuration de la sécurité et de l'AuthenticationStateProvider
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

var host = builder.Build();
await host.RunAsync();