using KanbanApp.Domain.Entities;
using KanbanApp.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace KanbanApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AutoRoleAssignmentService _roleService;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(AutoRoleAssignmentService roleService, UserManager<AppUser> userManager)
    {
        _roleService = roleService;
        _userManager = userManager;
    }

    private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// Returns the current user's ID and email — used client-side for ownership checks.
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        if (UserId == null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        return Ok(new
        {
            id          = user.Id,
            email       = user.Email ?? "",
            displayName = user.DisplayName,
            avatarUrl   = user.AvatarUrl
        });
    }

    /// Update display name.
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        if (UserId == null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        user.DisplayName = req.DisplayName?.Trim() ?? user.DisplayName;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });

        return Ok(new { displayName = user.DisplayName });
    }

    /// Change password.
    [HttpPost("me/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        if (UserId == null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, req.CurrentPassword, req.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });

        return Ok();
    }

    [HttpPost("assign-developer-role")]
    [Authorize]
    public async Task<IActionResult> AssignDeveloperRole()
    {
        if (UserId == null) return Unauthorized();
        await _roleService.AssignDeveloperRoleAsync(UserId);
        return Ok();
    }

    [HttpGet("my-roles")]
    [Authorize]
    public async Task<IActionResult> GetMyRoles()
    {
        if (UserId == null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles);
    }
}

public record UpdateProfileRequest([Required] string? DisplayName);
public record ChangePasswordRequest([Required] string CurrentPassword, [Required] string NewPassword);