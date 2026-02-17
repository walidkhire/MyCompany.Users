using Microsoft.AspNetCore.Http;
using MyCompany.Orders.Domain.Interfaces;
using System.Net.Http.Headers;

namespace MyCompany.Orders.Infrastructure.HttpClients
{

    public class UsersClient : IUsersClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> UserExistsAsync(Guid userId, string jwtToken)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(token))
            {
                // Ajouter le token dans l'Authorization header
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            }

            var res = await _http.GetAsync($"api/users/{userId}");
            return res.IsSuccessStatusCode;
        }
    }

}