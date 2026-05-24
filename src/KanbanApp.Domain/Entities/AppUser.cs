using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace KanbanApp.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        
        public string? AvatarUrl { get; set; }
        
        public ICollection<Board> OwnedBoards { get; set; } = new List<Board>();
        
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}