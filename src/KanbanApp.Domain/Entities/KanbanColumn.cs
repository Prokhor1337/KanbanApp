using KanbanApp.Domain.Common;
using System;
using System.Collections.Generic;

namespace KanbanApp.Domain.Entities
{
    /// Represents a column on a Kanban board (e.g., To Do, In Progress, Done).
    public class KanbanColumn : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        
        public int Order { get; set; }
        
        public Guid BoardId { get; set; }
        
        public Board Board { get; set; } = null!;
        
        public ICollection<TaskItem> Tasks { get; set; } = [];
    }
}