using KanbanApp.Application.DTOs;
using KanbanApp.Application.Interfaces;
using KanbanApp.Domain.Entities;
using KanbanApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KanbanApp.Application.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boardRepository;

        public BoardService(IBoardRepository boardRepository)
        {
            _boardRepository = boardRepository;
        }

        public async Task<BoardDto?> GetBoardByIdAsync(Guid id, CancellationToken ct = default)
        {
            var board = await _boardRepository.GetByIdAsync(id, ct);
            if (board == null) return null;

            return MapToDto(board);
        }

        public async Task<IEnumerable<BoardDto>> GetUserBoardsAsync(string userId, CancellationToken ct = default)
        {
            var boards = await _boardRepository.GetByOwnerIdAsync(userId, ct);
            return boards.Select(MapToDto);
        }
        
        public async Task<IEnumerable<BoardDto>> GetAllBoardsAsync(CancellationToken ct = default)
        {
            var boards = await _boardRepository.GetAllAsync(ct);
            return boards.Select(MapToDto);
        }

        public async Task<BoardDto> CreateBoardAsync(string userId, CreateBoardDto dto, CancellationToken ct = default)
        {
            var board = new Board
            {
                Title = dto.Title,
                Description = dto.Description,
                OwnerId = userId
            };

            await _boardRepository.AddAsync(board, ct);
            return MapToDto(board);
        }

        public async Task UpdateBoardAsync(Guid id, UpdateBoardDto dto, CancellationToken ct = default)
        {
            var board = await _boardRepository.GetByIdAsync(id, ct);
            if (board != null)
            {
                board.Title = dto.Title;
                board.Description = dto.Description;
                board.UpdatedAt = DateTime.UtcNow;

                await _boardRepository.UpdateAsync(board, ct);
            }
        }

        public async Task DeleteBoardAsync(Guid id, CancellationToken ct = default)
        {
            await _boardRepository.DeleteAsync(id, ct);
        }

        private static BoardDto MapToDto(Board board)
        {
            return new BoardDto
            {
                Id = board.Id,
                Title = board.Title,
                Description = board.Description,
                OwnerId = board.OwnerId,
                CreatedAt = board.CreatedAt,
                Columns = board.Columns.Select(c => new KanbanColumnDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Order = c.Order,
                    BoardId = c.BoardId,
                    Tasks = c.Tasks.Select(t => new TaskItemDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Priority = t.Priority.ToString(),
                        DueDate = t.DueDate,
                        Order = t.Order,
                        ColumnId = t.ColumnId,
                        AssigneeId = t.AssigneeId,
                        AssigneeName = t.Assignee?.DisplayName
                    }).ToList()
                }).ToList()
            };
        }
    }
}