using KanbanApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KanbanApp.Application.Interfaces
{
    public interface IColumnService
    {
        Task<IEnumerable<KanbanColumnDto>> GetColumnsByBoardIdAsync(Guid boardId, CancellationToken ct = default);
        Task<KanbanColumnDto> CreateColumnAsync(CreateColumnDto dto, CancellationToken ct = default);
        Task UpdateColumnAsync(Guid id, UpdateColumnDto dto, CancellationToken ct = default);
        Task DeleteColumnAsync(Guid id, CancellationToken ct = default);
    }
}