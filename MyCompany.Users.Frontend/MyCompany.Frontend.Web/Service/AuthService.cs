using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace MyCompany.Frontend.Web.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string TokenKey = "authToken";

        public AuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task LoginAsync(string email, string password, string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
    }

    // 🟢 Déclaration propre et explicite du DTO au bon endroit
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}