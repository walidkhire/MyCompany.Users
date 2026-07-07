using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyCompany.Frontend.Web.Security;
using MyCompany.Frontend.Web.Services;

namespace MyCompany.Frontend.Web.Handlers
{
    public class JwtHttpHandler : DelegatingHandler
    {
        private readonly AuthService _authService;
        private readonly NavigationManager _navigationManager;
        private readonly IServiceProvider _serviceProvider; // 💡 Évite les boucles d'injection au démarrage

        public JwtHttpHandler(AuthService authService, NavigationManager navigationManager, IServiceProvider serviceProvider)
        {
            _authService = authService;
            _navigationManager = navigationManager;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. On récupère le token depuis le LocalStorage
            var token = await _authService.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 2. On laisse la requête partir vers la Gateway YARP
            var response = await base.SendAsync(request, cancellationToken);

            // 3. 🔥 INTERCEPTION GLOBAL DES ERREURS HTTP
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Si l'API renvoie 401, le token est expiré ou corrompu
                await _authService.LogoutAsync();

                // On récupère le StateProvider pour forcer l'interface à repasser en mode Anonyme
                var authStateProvider = (CustomAuthStateProvider)_serviceProvider.GetService(typeof(AuthenticationStateProvider))!;
                authStateProvider.NotifyUserLogout();

                // Redirection immédiate vers la page d'accueil (formulaire de login)
                _navigationManager.NavigateTo("");
            }

            return response;
        }
    }
}