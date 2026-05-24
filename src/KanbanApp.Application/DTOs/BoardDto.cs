using System;
using System.Collections.Generic;

namespace KanbanApp.Application.DTOs
{
    public class BoardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ICollection<KanbanColumnDto> Columns { get; set; } = [];
    }

    public class CreateBoardDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateBoardDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}