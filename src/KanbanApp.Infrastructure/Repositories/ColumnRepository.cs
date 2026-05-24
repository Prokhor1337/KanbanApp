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
    /// SQL Server implementation of the IColumnRepository using Entity Framework Core.
    public class ColumnRepository : IColumnRepository
    {
        private readonly AppDbContext _context;

        public ColumnRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KanbanColumn?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Columns
                .Include(c => c.Tasks.OrderBy(t => t.Order))
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<IEnumerable<KanbanColumn>> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default)
        {
            return await _context.Columns
                .Where(c => c.BoardId == boardId)
                .OrderBy(c => c.Order)
                .ToListAsync(ct);
        }

        public async Task AddAsync(KanbanColumn column, CancellationToken ct = default)
        {
            await _context.Columns.AddAsync(column, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(KanbanColumn column, CancellationToken ct = default)
        {
            _context.Columns.Update(column);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var column = await _context.Columns.FindAsync(new object[] { id }, ct);
            if (column != null)
            {
                _context.Columns.Remove(column);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}