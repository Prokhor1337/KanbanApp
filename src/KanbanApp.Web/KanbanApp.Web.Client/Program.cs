using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using KanbanApp.Web.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

// Custom auth state provider for auto-login via JWT in localStorage
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();

// Default HttpClient pointing to the server's base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// API service — required for all board/team data fetching
builder.Services.AddScoped<KanbanApiService>();

// Global toast notification service
builder.Services.AddSingleton<ToastService>();

await builder.Build().RunAsync();