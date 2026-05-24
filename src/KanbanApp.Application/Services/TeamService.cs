using KanbanApp.Application.DTOs;
using KanbanApp.Application.Interfaces;
using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace KanbanApp.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepo;
        private readonly IBoardRepository _boardRepo;
        private readonly UserManager<AppUser> _userManager;

        public TeamService(ITeamRepository teamRepo, IBoardRepository boardRepo, UserManager<AppUser> userManager)
        {
            _teamRepo   = teamRepo;
            _boardRepo  = boardRepo;
            _userManager = userManager;
        }

        public async Task<TeamDto> CreateTeamAsync(string ownerId, CreateTeamDto dto)
        {
            var owner = await _userManager.FindByIdAsync(ownerId)
                ?? throw new InvalidOperationException("User not found.");

            var team = new Team
            {
                Name        = dto.Name,
                Description = dto.Description,
                OwnerId     = ownerId
            };

            await _teamRepo.AddAsync(team);

            // Auto-add owner as member with "Owner" role
            await _teamRepo.AddMemberAsync(new TeamMember
            {
                TeamId = team.Id,
                UserId = ownerId,
                Role   = "Owner"
            });

            return MapToDto(team, owner, 1, 0);
        }

        public async Task<TeamDetailDto?> GetTeamByIdAsync(Guid teamId, string userId)
        {
            var team = await _teamRepo.GetByIdWithDetailsAsync(teamId);
            if (team == null) return null;

            var isMember = team.OwnerId == userId || team.Members.Any(m => m.UserId == userId);
            if (!isMember) return null;

            return MapToDetailDto(team);
        }

        public async Task<IEnumerable<TeamDto>> GetUserTeamsAsync(string userId)
        {
            var teams = await _teamRepo.GetByUserIdAsync(userId);
            var result = new List<TeamDto>();

            foreach (var team in teams)
            {
                var owner = await _userManager.FindByIdAsync(team.OwnerId);
                result.Add(MapToDto(team, owner, team.Members.Count, team.TeamBoards.Count));
            }

            return result;
        }

        public async Task AddMemberAsync(Guid teamId, string requestingUserId, AddMemberDto dto)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");

            if (team.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the team owner can add members.");

            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new InvalidOperationException($"User with email '{dto.Email}' not found.");

            var alreadyMember = await _teamRepo.IsUserMemberAsync(teamId, user.Id);
            if (alreadyMember)
                throw new InvalidOperationException("User is already a member of this team.");

            await _teamRepo.AddMemberAsync(new TeamMember
            {
                TeamId = teamId,
                UserId = user.Id,
                Role   = "Member"
            });
        }

        public async Task RemoveMemberAsync(Guid teamId, string requestingUserId, string targetUserId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");

            if (team.OwnerId != requestingUserId && requestingUserId != targetUserId)
                throw new UnauthorizedAccessException("Only the owner can remove other members.");

            if (targetUserId == team.OwnerId)
                throw new InvalidOperationException("Cannot remove the team owner.");

            await _teamRepo.RemoveMemberAsync(teamId, targetUserId);
        }

        public async Task AddBoardToTeamAsync(Guid teamId, string requestingUserId, Guid boardId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");

            if (team.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the team owner can add boards.");

            var board = await _boardRepo.GetByIdAsync(boardId)
                ?? throw new InvalidOperationException("Board not found.");

            if (board.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("You can only add boards that you own.");

            await _teamRepo.AddBoardAsync(new TeamBoard { TeamId = teamId, BoardId = boardId });
        }

        public async Task RemoveBoardFromTeamAsync(Guid teamId, string requestingUserId, Guid boardId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");

            if (team.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the team owner can remove boards.");

            await _teamRepo.RemoveBoardAsync(teamId, boardId);
        }

        public async Task DeleteTeamAsync(Guid teamId, string requestingUserId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");

            if (team.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the team owner can delete the team.");

            await _teamRepo.DeleteAsync(teamId);
        }

        // ── Mapping helpers ───────────────────────────────────────────
        private static TeamDto MapToDto(Team team, AppUser? owner, int memberCount, int boardCount) => new()
        {
            Id          = team.Id,
            Name        = team.Name,
            Description = team.Description,
            OwnerId     = team.OwnerId,
            OwnerName   = owner?.DisplayName ?? owner?.Email ?? "Unknown",
            MemberCount = memberCount,
            BoardCount  = boardCount,
            InviteCode  = team.InviteCode
        };

        private static TeamDetailDto MapToDetailDto(Team team) => new()
        {
            Id          = team.Id,
            Name        = team.Name,
            Description = team.Description,
            OwnerId     = team.OwnerId,
            OwnerName   = team.Owner?.DisplayName ?? team.Owner?.Email ?? "Unknown",
            InviteCode  = team.InviteCode,
            Members     = team.Members.Select(m => new TeamMemberDto
            {
                UserId      = m.UserId,
                DisplayName = m.User?.DisplayName ?? m.User?.Email ?? "Unknown",
                Email       = m.User?.Email ?? string.Empty,
                Role        = m.Role
            }).ToList(),
            Boards = team.TeamBoards.Select(tb => new BoardDto
            {
                Id          = tb.Board.Id,
                Title       = tb.Board.Title,
                Description = tb.Board.Description,
                OwnerId     = tb.Board.OwnerId,
                CreatedAt   = tb.Board.CreatedAt,
                Columns     = []
            }).ToList()
        };

        // ── Invite Link ───────────────────────────────────────────────
        public async Task<Guid> GenerateInviteLinkAsync(Guid teamId, string requestingUserId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");
            if (team.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the owner can generate an invite link.");

            var code = Guid.NewGuid();
            await _teamRepo.UpdateInviteCodeAsync(teamId, code);
            return code;
        }

        public async Task RevokeInviteLinkAsync(Guid teamId, string requestingUserId)
        {
            var team = await _teamRepo.GetByIdAsync(teamId)
                ?? throw new InvalidOperationException("Team not found.");
            if (team.OwnerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the owner can revoke the invite link.");

            await _teamRepo.UpdateInviteCodeAsync(teamId, null);
        }

        public async Task<TeamInviteInfoDto?> GetTeamByInviteCodeAsync(Guid inviteCode)
        {
            var team = await _teamRepo.GetByInviteCodeAsync(inviteCode);
            if (team == null) return null;

            return new TeamInviteInfoDto
            {
                TeamId      = team.Id,
                TeamName    = team.Name,
                Description = team.Description,
                MemberCount = team.Members.Count
            };
        }

        public async Task JoinViaInviteCodeAsync(Guid inviteCode, string userId)
        {
            var team = await _teamRepo.GetByInviteCodeAsync(inviteCode)
                ?? throw new InvalidOperationException("Invalid or expired invite link.");

            var alreadyMember = await _teamRepo.IsUserMemberAsync(team.Id, userId);
            if (alreadyMember) return; // silently skip if already joined

            await _teamRepo.AddMemberAsync(new TeamMember
            {
                TeamId = team.Id,
                UserId = userId,
                Role   = "Member"
            });
        }
    }
}
