using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Interfaces;
using KanbanApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _context;

        public TeamRepository(AppDbContext context) => _context = context;

        public async Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Teams.FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<Team?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
            => await _context.Teams
                .Include(t => t.Owner)
                .Include(t => t.Members)
                    .ThenInclude(m => m.User)
                .Include(t => t.TeamBoards)
                    .ThenInclude(tb => tb.Board)
                        .ThenInclude(b => b.Columns)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

        public async Task<IEnumerable<Team>> GetByUserIdAsync(string userId, CancellationToken ct = default)
            => await _context.Teams
                .Include(t => t.Members)
                .Include(t => t.TeamBoards)
                .Where(t => t.OwnerId == userId || t.Members.Any(m => m.UserId == userId))
                .ToListAsync(ct);

        public async Task<bool> IsUserMemberAsync(Guid teamId, string userId, CancellationToken ct = default)
        {
            var team = await _context.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == teamId, ct);

            if (team == null) return false;
            return team.OwnerId == userId || team.Members.Any(m => m.UserId == userId);
        }

        public async Task AddAsync(Team team, CancellationToken ct = default)
        {
            await _context.Teams.AddAsync(team, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Team team, CancellationToken ct = default)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var team = await _context.Teams.FindAsync([id], ct);
            if (team != null)
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task AddMemberAsync(TeamMember member, CancellationToken ct = default)
        {
            await _context.TeamMembers.AddAsync(member, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task RemoveMemberAsync(Guid teamId, string userId, CancellationToken ct = default)
        {
            var member = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId, ct);
            if (member != null)
            {
                _context.TeamMembers.Remove(member);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task AddBoardAsync(TeamBoard teamBoard, CancellationToken ct = default)
        {
            await _context.TeamBoards.AddAsync(teamBoard, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task RemoveBoardAsync(Guid teamId, Guid boardId, CancellationToken ct = default)
        {
            var tb = await _context.TeamBoards
                .FirstOrDefaultAsync(x => x.TeamId == teamId && x.BoardId == boardId, ct);
            if (tb != null)
            {
                _context.TeamBoards.Remove(tb);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> IsBoardInAnyTeamOfUserAsync(Guid boardId, string userId, CancellationToken ct = default)
            => await _context.TeamBoards
                .AnyAsync(tb => tb.BoardId == boardId &&
                    (tb.Team.OwnerId == userId || tb.Team.Members.Any(m => m.UserId == userId)), ct);

        public async Task<Team?> GetByInviteCodeAsync(Guid inviteCode, CancellationToken ct = default)
            => await _context.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.InviteCode == inviteCode, ct);

        public async Task UpdateInviteCodeAsync(Guid teamId, Guid? code, CancellationToken ct = default)
        {
            var team = await _context.Teams.FindAsync([teamId], ct);
            if (team != null)
            {
                team.InviteCode = code;
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
