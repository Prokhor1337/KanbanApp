using KanbanApp.Domain.Common;
using System.Collections.Generic;

namespace KanbanApp.Domain.Entities
{
    /// Represents a team of users who share boards and collaborate together.
    public class Team : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// The user who created the team (always has Owner role).
        public string OwnerId { get; set; } = string.Empty;

        public AppUser Owner { get; set; } = null!;

        /// Random code for invite-link feature (null = links disabled)
        public Guid? InviteCode { get; set; }

        public ICollection<TeamMember> Members { get; set; } = [];

        public ICollection<TeamBoard> TeamBoards { get; set; } = [];
    }
}
