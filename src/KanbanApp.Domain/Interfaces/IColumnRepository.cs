using KanbanApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KanbanApp.Domain.Interfaces
{
    public interface IColumnRepository
    {
        Task<KanbanColumn?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<KanbanColumn>> GetByBoardIdAsync(Guid boardId, CancellationToken ct = default);
        Task AddAsync(KanbanColumn column, CancellationToken ct = default);
        Task UpdateAsync(KanbanColumn column, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}