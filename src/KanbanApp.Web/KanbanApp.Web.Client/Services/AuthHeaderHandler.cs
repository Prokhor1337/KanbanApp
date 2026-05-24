using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace KanbanApp.Web.Client.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthHeaderHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken"); // GetItemAsStringAsync avoids type issues
            if (!string.IsNullOrWhiteSpace(token))
            {
                // Strip any surrounding quotes that may come from JSON serialization
                token = token.Trim('"');
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // LocalStorage is not available during SSR — silently skip
        }
    
        return await base.SendAsync(request, cancellationToken);
    }
}