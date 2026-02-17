using System.Net.Http.Json;
using MyCompany.Users.Frontend.Models;
using Blazored.LocalStorage;

namespace MyCompany.Users.Frontend.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        // 🔹 Récupérer le token et l'utilisateur courant
        public async Task<User?> GetCurrentUserAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("jwt_token");
            if (string.IsNullOrEmpty(token)) return null;

            var email = await _localStorage.GetItemAsync<string>("user_email");
            return new User { Email = email };
        }

        // 🔹 Login vers le backend
        public async Task<bool> AuthenticateAsync(string email, string password)
        {

            // Préparer le DTO
            var loginDto = new
            {
                Email = email,
                Password = password
            };

            // Envoyer vers le backend
            var response = await _http.PostAsJsonAsync("https://localhost:7070/api/auth/login", loginDto);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (result == null) return false;

            await _localStorage.SetItemAsync("jwt_token", result.Token);
            await _localStorage.SetItemAsync("user_email", email);

            // 🔹 Peut-être notifier AuthStateProvider ici si utilisé
            return true;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("jwt_token");
            await _localStorage.RemoveItemAsync("user_email");
        }
    }
}
