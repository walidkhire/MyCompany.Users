using MyCompany.Users.Frontend.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MyCompany.Users.Frontend.Services
{
    using Blazored.LocalStorage;
    using MyCompany.Users.Frontend.Models;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;

    public class UserService : IUserService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        public UserService(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("jwt_token");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var users = await _http.GetFromJsonAsync<List<UserDto>>("api/users");
            return users ?? new List<UserDto>();
        }
    }

}
