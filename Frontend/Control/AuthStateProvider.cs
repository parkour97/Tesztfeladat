using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Frontend.Control
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
        private string? _username;

        public void MarkUserAsAuthenticated(string username)
        {
            _username = username;

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }, "apiauth_type");

            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void MarkUserAsLoggedOut()
        {
            _username = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (string.IsNullOrEmpty(_username))
                return Task.FromResult(new AuthenticationState(_anonymous));

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, _username)
            }, "apiauth_type");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
