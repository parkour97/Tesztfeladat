using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace Frontend.Control
{
    public class AuthStateProvider
    {
        private ClaimsPrincipal currentUser;

        public AuthStateProvider()
        {
        }

        public string? CurrentUserName => currentUser.Identity?.Name;

        public async Task MarkUserAsAuthenticated(HttpContext? httpContext, string username)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            currentUser = new ClaimsPrincipal(identity);

            await httpContext.SignInAsync(currentUser);
        }

        public async Task MarkUserAsLoggedOut(HttpContext? httpContext)
        {
            await httpContext.SignOutAsync();
            currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }
    }

}
