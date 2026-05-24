using KanbanApp.Domain.Entities;

namespace KanbanApp.Domain.Interfaces
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Team?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Team>> GetByUserIdAsync(string userId, CancellationToken ct = default);
        Task<bool> IsUserMemberAsync(Guid teamId, string userId, CancellationToken ct = default);
        Task AddAsync(Team team, CancellationToken ct = default);
        Task UpdateAsync(Team team, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task AddMemberAsync(TeamMember member, CancellationToken ct = default);
        Task RemoveMemberAsync(Guid teamId, string userId, CancellationToken ct = default);
        Task AddBoardAsync(TeamBoard teamBoard, CancellationToken ct = default);
        Task RemoveBoardAsync(Guid teamId, Guid boardId, CancellationToken ct = default);
        Task<bool> IsBoardInAnyTeamOfUserAsync(Guid boardId, string userId, CancellationToken ct = default);
        Task<Team?> GetByInviteCodeAsync(Guid inviteCode, CancellationToken ct = default);
        Task UpdateInviteCodeAsync(Guid teamId, Guid? code, CancellationToken ct = default);
    }
}
