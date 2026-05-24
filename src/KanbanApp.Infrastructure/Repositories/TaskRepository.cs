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
    /// SQL Server implementation of the ITaskRepository using Entity Framework Core.
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Tasks
                .Include(t => t.Assignee)
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<IEnumerable<TaskItem>> GetByColumnIdAsync(Guid columnId, CancellationToken ct = default)
        {
            return await _context.Tasks
                .Where(t => t.ColumnId == columnId)
                .OrderBy(t => t.Order)
                .ToListAsync(ct);
        }

        public async Task AddAsync(TaskItem task, CancellationToken ct = default)
        {
            await _context.Tasks.AddAsync(task, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var task = await _context.Tasks.FindAsync(new object[] { id }, ct);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}