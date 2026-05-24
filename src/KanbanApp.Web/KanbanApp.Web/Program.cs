using KanbanApp.Application.Interfaces;
using KanbanApp.Application.Services;
using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Interfaces;
using KanbanApp.Infrastructure.Data;
using KanbanApp.Infrastructure.Repositories;
using KanbanApp.Web.Components;
using KanbanApp.Web.Middleware;
using KanbanApp.Web.Services;
using KanbanApp.Web.Hubs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Blazored.LocalStorage;
using Serilog;
using Npgsql.EntityFrameworkCore.PostgreSQL;

// ─── Serilog ──────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/kanban-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ─── 1. Database ──────────────────────────────────────────────────────
// Railway provides DATABASE_URL; fallback to appsettings for local dev
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ─── 2. DataProtection ───────────────────────────────────────────
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "AuthKeys")));

// ─── 3. Identity + Bearer ────────────────────────────────────────
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddApiEndpoints();

builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

// ─── 4. Repositories and Services ────────────────────────────────
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IColumnRepository, ColumnRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IColumnService, ColumnService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<AutoRoleAssignmentService>();

// ─── 5. Blazor + WASM ────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// Toast сервис нужен и на сервере (Blazor рендерит компоненты server-side)
builder.Services.AddSingleton<KanbanApp.Web.Client.Services.ToastService>();

// ─── 6. Controllers + SignalR ────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ─── 7. Swagger ──────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KanbanApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        In = ParameterLocation.Header,
        Description = "Insert the token obtained during login"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

// ─── BUILD ────────────────────────────────────────────────────────
var app = builder.Build();

// ─── 8. Auto-migrate + Seed Roles on Startup ────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    // Apply any pending EF Core migrations automatically (needed for Railway deploy)
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    string[] roles = ["Admin", "Developer"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Default admin account
    var adminEmail = "admin@kanban.com";
    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new AppUser { UserName = adminEmail, Email = adminEmail, DisplayName = "Admin" };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// ─── 9. Middleware pipeline ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();

// ─── .NET 8 IDENTITY FIX: Enrich token with roles from DB ───
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.GetUserAsync(context.User);
        
        if (user != null)
        {
            var roles = await userManager.GetRolesAsync(user);
            var identity = context.User.Identity as System.Security.Claims.ClaimsIdentity;
            
            foreach (var role in roles)
            {
                if (!identity!.HasClaim(System.Security.Claims.ClaimTypes.Role, role))
                {
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                }
            }
        }
    }
    await next();
});

app.UseAuthorization();

app.MapIdentityApi<AppUser>();
app.MapControllers();
app.MapHub<BoardHub>("/boardhub");

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(KanbanApp.Web.Client._Imports).Assembly)
    .DisableAntiforgery();

app.Run();