using KanbanApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KanbanApp.Application.Interfaces
{
    public interface IBoardService
    {
        Task<BoardDto?> GetBoardByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<BoardDto>> GetUserBoardsAsync(string userId, CancellationToken ct = default);
        Task<IEnumerable<BoardDto>> GetAllBoardsAsync(CancellationToken ct = default);
        Task<BoardDto> CreateBoardAsync(string userId, CreateBoardDto dto, CancellationToken ct = default);
        Task UpdateBoardAsync(Guid id, UpdateBoardDto dto, CancellationToken ct = default);
        Task DeleteBoardAsync(Guid id, CancellationToken ct = default);
    }
}