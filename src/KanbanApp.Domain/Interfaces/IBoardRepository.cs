using KanbanApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KanbanApp.Domain.Interfaces
{
    public interface IBoardRepository
    {
        Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Board>> GetByOwnerIdAsync(string userId, CancellationToken ct = default);
        Task AddAsync(Board board, CancellationToken ct = default);
        Task UpdateAsync(Board board, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Board>> GetAllAsync(CancellationToken ct = default);
    }
}