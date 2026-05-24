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
    public class ColumnService : IColumnService
    {
        private readonly IColumnRepository _columnRepository;
        private readonly ITaskRepository _taskRepository; // task repository for cascade delete

        // Inject both repositories through the constructor
        public ColumnService(IColumnRepository columnRepository, ITaskRepository taskRepository)
        {
            _columnRepository = columnRepository;
            _taskRepository = taskRepository;
        }

        public async Task<IEnumerable<KanbanColumnDto>> GetColumnsByBoardIdAsync(Guid boardId,
            CancellationToken ct = default)
        {
            var columns = await _columnRepository.GetByBoardIdAsync(boardId, ct);
            return columns.Select(c => new KanbanColumnDto
            {
                Id = c.Id,
                Title = c.Title,
                Order = c.Order,
                BoardId = c.BoardId
            });
        }

        public async Task<KanbanColumnDto> CreateColumnAsync(CreateColumnDto dto, CancellationToken ct = default)
        {
            var column = new KanbanColumn
            {
                Title = dto.Title,
                BoardId = dto.BoardId,
                Order = dto.Order
            };

            await _columnRepository.AddAsync(column, ct);

            return new KanbanColumnDto
            {
                Id = column.Id,
                Title = column.Title,
                Order = column.Order,
                BoardId = column.BoardId
            };
        }

        public async Task UpdateColumnAsync(Guid id, UpdateColumnDto dto, CancellationToken ct = default)
        {
            var column = await _columnRepository.GetByIdAsync(id, ct);
            if (column != null)
            {
                column.Title = dto.Title;
                column.Order = dto.Order;
                column.UpdatedAt = DateTime.UtcNow;

                await _columnRepository.UpdateAsync(column, ct);
            }
        }

        public async Task DeleteColumnAsync(Guid id, CancellationToken ct = default)
        {
            var column = await _columnRepository.GetByIdAsync(id, ct);
            if (column != null)
            {
                // Fetch tasks for this column (note the method name: GetByColumnIdAsync)
                var tasksInColumn = await _taskRepository.GetByColumnIdAsync(id, ct);

                // Delete each task by its ID
                foreach (var task in tasksInColumn)
                {
                    await _taskRepository.DeleteAsync(task.Id, ct);
                }

                // Delete the column itself by its ID
                await _columnRepository.DeleteAsync(id, ct);
            }
        }
    }
}