namespace KanbanApp.Domain.Entities
{
    /// Join table linking a Team to a Board (shared board).
    public class TeamBoard
    {
        public Guid TeamId { get; set; }

        public Team Team { get; set; } = null!;

        public Guid BoardId { get; set; }

        public Board Board { get; set; } = null!;
    }
}
