using KanbanApp.Application.DTOs;

namespace KanbanApp.Application.Interfaces
{
    public interface ITeamService
    {
        Task<TeamDto> CreateTeamAsync(string ownerId, CreateTeamDto dto);
        Task<TeamDetailDto?> GetTeamByIdAsync(Guid teamId, string userId);
        Task<IEnumerable<TeamDto>> GetUserTeamsAsync(string userId);
        Task AddMemberAsync(Guid teamId, string requestingUserId, AddMemberDto dto);
        Task RemoveMemberAsync(Guid teamId, string requestingUserId, string targetUserId);
        Task AddBoardToTeamAsync(Guid teamId, string requestingUserId, Guid boardId);
        Task RemoveBoardFromTeamAsync(Guid teamId, string requestingUserId, Guid boardId);
        Task DeleteTeamAsync(Guid teamId, string requestingUserId);
        Task<Guid> GenerateInviteLinkAsync(Guid teamId, string requestingUserId);
        Task RevokeInviteLinkAsync(Guid teamId, string requestingUserId);
        Task<TeamInviteInfoDto?> GetTeamByInviteCodeAsync(Guid inviteCode);
        Task JoinViaInviteCodeAsync(Guid inviteCode, string userId);
    }
}
