using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyCompany.Users.Frontend;
using MyCompany.Users.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// 🔹 HTTP client pour ton backend
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7070/")
});

// 🔹 Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// 🔹 AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// 🔹 CustomAuthStateProvider si tu l’utilises
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());

await builder.Build().RunAsync();
