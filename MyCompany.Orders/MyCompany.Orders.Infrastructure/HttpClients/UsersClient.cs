using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MyCompany.Orders.Infrastructure.HttpClients
{
    public class UsersClient : IUsersClient
    {
        private readonly HttpClient _http;

        public UsersClient(HttpClient http)
        {
            _http = http;
            // 🔹 Configuration de l'adresse de base du microservice Users (Port HTTPS direct)
            _http.BaseAddress = new Uri("https://localhost:7070/");
        }

        public async Task<bool> UserExistsAsync(Guid userId, string jwtToken)
        {
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                return false;
            }

            // 🔹 Nettoyer les anciens headers pour éviter l'accumulation
            _http.DefaultRequestHeaders.Authorization = null;

            // 🔹 Injecter le Token JWT reçu pour s'authentifier auprès de Users.API
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            try
            {
                // L'URL finale sera : https://localhost:7070/api/users/{userId}
                var res = await _http.GetAsync($"api/users/{userId}");
                return res.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // En cas de coupure réseau ou serveur Users éteint
                return false;
            }
        }
    }
}