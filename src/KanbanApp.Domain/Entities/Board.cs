using KanbanApp.Domain.Common;
using System.Collections.Generic;

namespace KanbanApp.Domain.Entities
{
    /// Represents a Kanban board that contains multiple columns and belongs to a specific user.
    public class Board : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string OwnerId { get; set; } = string.Empty;
        
        public AppUser Owner { get; set; } = null!;
        
        public ICollection<KanbanColumn> Columns { get; set; } = [];
    }
}