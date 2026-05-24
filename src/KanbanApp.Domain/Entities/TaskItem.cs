using KanbanApp.Domain.Common;
using System;

namespace KanbanApp.Domain.Entities
{
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Represents an individual task card on the Kanban board.
    /// </summary>
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        public DateTime? DueDate { get; set; }
        
        public int Order { get; set; }
        
        public Guid ColumnId { get; set; }
        
        public KanbanColumn Column { get; set; } = null!;
        
        public string? AssigneeId { get; set; }
        
        public AppUser? Assignee { get; set; }

        // Note: Tags and Comments from the plan (Step 2.6) are commented out
        // to avoid new migration errors until those entities are created.
        // public ICollection<TaskTag> Tags { get; set; } = [];
        // public ICollection<TaskComment> Comments { get; set; } = [];
    }
}