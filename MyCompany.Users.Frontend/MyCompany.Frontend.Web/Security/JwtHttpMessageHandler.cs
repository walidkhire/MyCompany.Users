using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection; // 💡 Obligatoire pour le GetRequiredService
using MyCompany.Frontend.Web.Services;

namespace MyCompany.Frontend.Web.Security
{
    public class JwtHttpMessageHandler : DelegatingHandler
    {



        private readonly IServiceProvider _serviceProvider;

        // 1. 🔥 On injecte l'IServiceProvider à la place de l'AuthService direct
        public JwtHttpMessageHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 2. 🔥 On récupère l'AuthService à la demande pour casser la boucle d'injection
            var authService = _serviceProvider.GetRequiredService<AuthService>();

            var token = await authService.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}