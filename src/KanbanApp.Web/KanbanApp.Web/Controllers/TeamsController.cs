using System.Security.Claims;
using KanbanApp.Application.DTOs;
using KanbanApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanApp.Web.Controllers
{
    [Authorize]
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly ILogger<TeamsController> _logger;

        public TeamsController(ITeamService teamService, ILogger<TeamsController> logger)
        {
            _teamService = teamService;
            _logger      = logger;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET /api/teams — get all teams for the current user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetMyTeams()
        {
            if (UserId == null) return Unauthorized();
            var teams = await _teamService.GetUserTeamsAsync(UserId);
            return Ok(teams);
        }

        // GET /api/teams/{id} — get team details (members + boards)
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TeamDetailDto>> GetTeam(Guid id)
        {
            if (UserId == null) return Unauthorized();
            var team = await _teamService.GetTeamByIdAsync(id, UserId);
            if (team == null) return NotFound();
            return Ok(team);
        }

        // POST /api/teams — create a new team
        [HttpPost]
        public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamDto dto)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                var team = await _teamService.CreateTeamAsync(UserId, dto);
                return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team");
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE /api/teams/{id} — delete team (owner only)
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteTeam(Guid id)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                await _teamService.DeleteTeamAsync(id, UserId);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // POST /api/teams/{id}/members — add member by email
        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto dto)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                await _teamService.AddMemberAsync(id, UserId, dto);
                return Ok();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // DELETE /api/teams/{id}/members/{userId} — remove member
        [HttpDelete("{id:guid}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid id, string memberId)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                await _teamService.RemoveMemberAsync(id, UserId, memberId);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // POST /api/teams/{id}/boards/{boardId} — attach a board
        [HttpPost("{id:guid}/boards/{boardId:guid}")]
        public async Task<IActionResult> AddBoard(Guid id, Guid boardId)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                await _teamService.AddBoardToTeamAsync(id, UserId, boardId);
                return Ok();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // DELETE /api/teams/{id}/boards/{boardId} — detach a board
        [HttpDelete("{id:guid}/boards/{boardId:guid}")]
        public async Task<IActionResult> RemoveBoard(Guid id, Guid boardId)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                await _teamService.RemoveBoardFromTeamAsync(id, UserId, boardId);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // POST /api/teams/{id}/invite — generate invite code (returns the code)
        [HttpPost("{id:guid}/invite")]
        public async Task<ActionResult<object>> GenerateInvite(Guid id)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                var code = await _teamService.GenerateInviteLinkAsync(id, UserId);
                return Ok(new { inviteCode = code });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // DELETE /api/teams/{id}/invite — revoke invite code
        [HttpDelete("{id:guid}/invite")]
        public async Task<IActionResult> RevokeInvite(Guid id)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                await _teamService.RevokeInviteLinkAsync(id, UserId);
                return NoContent();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        // GET /api/teams/join/{code} — get team info by invite code (public, no auth needed)
        [HttpGet("join/{code:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<TeamInviteInfoDto>> GetInviteInfo(Guid code)
        {
            var info = await _teamService.GetTeamByInviteCodeAsync(code);
            if (info == null) return NotFound(new { error = "Invalid or expired invite link." });
            return Ok(info);
        }

        // POST /api/teams/join/{code} — join team via invite code (requires auth)
        [HttpPost("join/{code:guid}")]
        public async Task<IActionResult> JoinViaInvite(Guid code)
        {
            if (UserId == null) return Unauthorized();
            try
            {
                await _teamService.JoinViaInviteCodeAsync(code, UserId);
                var info = await _teamService.GetTeamByInviteCodeAsync(code);
                return Ok(new { teamId = info?.TeamId });
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }
    }
}
