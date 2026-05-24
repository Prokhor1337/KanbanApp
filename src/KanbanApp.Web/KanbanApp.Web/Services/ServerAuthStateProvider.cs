using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace KanbanApp.Web.Services
{
    // A dummy provider for the server to survive pre-rendering.
    // Since the server cannot read the browser's LocalStorage, 
    // it assumes the user is an unauthenticated guest during the initial HTML generation.
    // Once WebAssembly loads in the browser, the Client's CustomAuthStateProvider takes over.
    public class ServerAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Return an empty, unauthenticated user
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult(new AuthenticationState(anonymousUser));
        }
    }
}