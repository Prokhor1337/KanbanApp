using KanbanApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace KanbanApp.Web.Services;

public class AutoRoleAssignmentService
{
    private readonly UserManager<AppUser> _userManager;

    public AutoRoleAssignmentService(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task AssignDeveloperRoleAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        if (!await _userManager.IsInRoleAsync(user, "Developer"))
            await _userManager.AddToRoleAsync(user, "Developer");
    }
}