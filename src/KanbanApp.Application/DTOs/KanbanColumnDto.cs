using System;
using System.Collections.Generic;

namespace KanbanApp.Application.DTOs
{
    public class KanbanColumnDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public Guid BoardId { get; set; }
        public ICollection<TaskItemDto> Tasks { get; set; } = [];
    }

    public class CreateColumnDto
    {
        public string Title { get; set; } = string.Empty;
        public Guid BoardId { get; set; }
        public int Order { get; set; }
    }

    public class UpdateColumnDto
    {
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}