using KanbanApp.Domain.Common;

namespace KanbanApp.Domain.Entities
{
    /// Represents a user's membership in a team with an optional role.
    public class TeamMember : BaseEntity
    {
        public Guid TeamId { get; set; }

        public Team Team { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public AppUser User { get; set; } = null!;

        /// Role within the team: "Owner" or "Member"
        public string Role { get; set; } = "Member";
    }
}
