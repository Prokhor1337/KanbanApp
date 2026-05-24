using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace KanbanApp.Web.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            token = token.Replace("\"", "");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Parse claims directly from JWT payload
            var claims = ParseClaimsFromJwt(token);

            // Fetch roles from the API and add them
            try
            {
                var roles = await _httpClient.GetFromJsonAsync<List<string>>("/api/account/my-roles");
                if (roles != null)
                {
                    foreach (var role in roles)
                        claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            catch
            {
                // If the API is unavailable, fall back to basic access
            }

            // CRITICAL: Tell Blazor where to look for roles in the JWT
            var identity = new ClaimsIdentity(claims, "bearer", ClaimTypes.Name, ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        /// Parses all claims from the JWT payload (second base64url segment).
        public static List<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length < 2) return claims;

                var payload = parts[1];
                // Pad base64url → standard base64
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var doc = JsonDocument.Parse(json);

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var type = prop.Name;
                    var value = prop.Value.ValueKind == JsonValueKind.String
                        ? prop.Value.GetString() ?? ""
                        : prop.Value.ToString();

                    // Map JWT standard claims → .NET ClaimTypes
                    if (type == "sub")
                    {
                        // sub → NameIdentifier so owner checks work
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, value));
                    }
                    else if (type == "name")
                    {
                        claims.Add(new Claim(ClaimTypes.Name, value));
                    }
                    else if (type == "email")
                    {
                        claims.Add(new Claim(ClaimTypes.Email, value));
                    }
                    else if (type == "role" || type == ClaimTypes.Role)
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var r in prop.Value.EnumerateArray())
                                claims.Add(new Claim(ClaimTypes.Role, r.GetString() ?? ""));
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, value));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(type, value));
                    }
                }

                // Fallback: ensure Name claim exists
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    var fallback = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                                ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                                ?? "User";
                    claims.Add(new Claim(ClaimTypes.Name, fallback));
                }
            }
            catch
            {
                claims.Add(new Claim(ClaimTypes.Name, "AuthenticatedUser"));
            }

            return claims;
        }

        public void NotifyUserAuthentication(string token)
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }
    }
}