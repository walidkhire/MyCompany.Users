using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyCompany.Frontend.Web.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationState _anonymous;

        public CustomAuthStateProvider(MyCompany.Frontend.Web.Services.AuthService authService)
        {
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 🟢 FORCE un retour immédiat sans aucun traitement pour casser le blocage à 100%
            return Task.FromResult(_anonymous);
        }

        public void NotifyUserAuthentication(string token)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Utilisateur") }, "jwtAuthType");
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }
    }
}