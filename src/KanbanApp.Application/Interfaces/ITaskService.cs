using KanbanApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KanbanApp.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskItemDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<TaskItemDto>> GetTasksByColumnIdAsync(Guid columnId, CancellationToken ct = default);
        Task<TaskItemDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken ct = default);
        Task UpdateTaskAsync(Guid id, UpdateTaskDto dto, CancellationToken ct = default);
        Task DeleteTaskAsync(Guid id, CancellationToken ct = default);
        Task MoveTaskAsync(Guid taskId, Guid targetColumnId, int newOrder, CancellationToken ct = default);
    }
}