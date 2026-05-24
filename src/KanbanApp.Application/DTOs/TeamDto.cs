namespace KanbanApp.Application.DTOs
{
    public class TeamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int BoardCount { get; set; }
        public Guid? InviteCode { get; set; }
    }

    public class TeamMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";
    }

    public class TeamDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public Guid? InviteCode { get; set; }
        public List<TeamMemberDto> Members { get; set; } = [];
        public List<BoardDto> Boards { get; set; } = [];
    }

    public class CreateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AddMemberDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class TeamInviteInfoDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MemberCount { get; set; }
    }
}
