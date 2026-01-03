using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Frontend.Control
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        private string? _token;

        public void MarkUserAsAuthenticated(string token)
        {
            _token = token;
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "User")
            }, "apiauth_type");

            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void MarkUserAsLoggedOut()
        {
            _token = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (string.IsNullOrEmpty(_token)) return Task.FromResult(new AuthenticationState(_anonymous));

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "User")
            }, "apiauth_type");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
