using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Interfaces;
using KanbanApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KanbanApp.Infrastructure.Repositories
{
    /// SQL Server implementation of the IBoardRepository using Entity Framework Core.
    public class BoardRepository : IBoardRepository
    {
        private readonly AppDbContext _context;

        public BoardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // We use Include to eagerly load columns and tasks inside them for the full Kanban view.
            return await _context.Boards
                .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Tasks.OrderBy(t => t.Order))
                .FirstOrDefaultAsync(b => b.Id == id, ct);
        }

        public async Task<IEnumerable<Board>> GetByOwnerIdAsync(string userId, CancellationToken ct = default)
        {
            return await _context.Boards
                .Where(b => b.OwnerId == userId)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Board board, CancellationToken ct = default)
        {
            await _context.Boards.AddAsync(board, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Board board, CancellationToken ct = default)
        {
            _context.Boards.Update(board);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var board = await _context.Boards
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tasks)
                .FirstOrDefaultAsync(b => b.Id == id, ct);

            if (board != null)
            {
                // Manually remove tasks and columns so cascade works across all providers
                foreach (var col in board.Columns)
                    _context.Tasks.RemoveRange(col.Tasks);
                _context.Columns.RemoveRange(board.Columns);

                // Remove TeamBoard join rows (cascade from DB side, but explicit is safer)
                var teamBoards = _context.TeamBoards.Where(tb => tb.BoardId == id);
                _context.TeamBoards.RemoveRange(teamBoards);

                _context.Boards.Remove(board);
                await _context.SaveChangesAsync(ct);
            }
        }
        public async Task<IEnumerable<Board>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Boards
                .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Tasks.OrderBy(t => t.Order))
                .ToListAsync(ct);
        }
    }
}