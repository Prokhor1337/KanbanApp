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
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<TaskItemDto?> GetTaskByIdAsync(Guid id, CancellationToken ct = default)
        {
            var task = await _taskRepository.GetByIdAsync(id, ct);
            if (task == null) return null;

            return MapToDto(task);
        }

        public async Task<IEnumerable<TaskItemDto>> GetTasksByColumnIdAsync(Guid columnId, CancellationToken ct = default)
        {
            var tasks = await _taskRepository.GetByColumnIdAsync(columnId, ct);
            return tasks.Select(MapToDto);
        }

        public async Task<TaskItemDto> CreateTaskAsync(CreateTaskDto dto, CancellationToken ct = default)
        {
            var priority = Enum.TryParse<TaskPriority>(dto.Priority, out var parsedPriority) 
                ? parsedPriority 
                : TaskPriority.Medium;

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = priority,
                DueDate = dto.DueDate,
                ColumnId = dto.ColumnId,
                // New tasks are always appended to the end of the list (ordering logic can be improved later)
                Order = 999 
            };

            await _taskRepository.AddAsync(task, ct);
            return MapToDto(task);
        }

        public async Task UpdateTaskAsync(Guid id, UpdateTaskDto dto, CancellationToken ct = default)
        {
            var task = await _taskRepository.GetByIdAsync(id, ct);
            if (task != null)
            {
                var priority = Enum.TryParse<TaskPriority>(dto.Priority, out var parsedPriority) 
                    ? parsedPriority 
                    : TaskPriority.Medium;

                task.Title = dto.Title;
                task.Description = dto.Description;
                task.Priority = priority;
                task.DueDate = dto.DueDate;
                task.ColumnId = dto.ColumnId;
                task.Order = dto.Order;
                task.AssigneeId = dto.AssigneeId;
                task.UpdatedAt = DateTime.UtcNow;

                await _taskRepository.UpdateAsync(task, ct);
            }
        }

        public async Task DeleteTaskAsync(Guid id, CancellationToken ct = default)
        {
            await _taskRepository.DeleteAsync(id, ct);
        }

        public async Task MoveTaskAsync(Guid taskId, Guid targetColumnId, int newOrder, CancellationToken ct = default)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, ct);
            if (task != null)
            {
                task.ColumnId = targetColumnId;
                task.Order = newOrder;
                task.UpdatedAt = DateTime.UtcNow;
                
                // Ideally we would recalculate Order for other tasks in the column,
                // but for the current MVP we simply update the current task.
                await _taskRepository.UpdateAsync(task, ct);
            }
        }

        private static TaskItemDto MapToDto(TaskItem task)
        {
            return new TaskItemDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority.ToString(),
                DueDate = task.DueDate,
                Order = task.Order,
                ColumnId = task.ColumnId,
                AssigneeId = task.AssigneeId,
                AssigneeName = task.Assignee?.DisplayName
            };
        }
    }
}