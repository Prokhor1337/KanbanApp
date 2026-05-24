using KanbanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanApp.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Database configuration for the TaskItem entity using Fluent API.
    /// </summary>
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(t => t.Description)
                .HasMaxLength(2000);

            builder.Property(t => t.Priority)
                .IsRequired()
                .HasConversion<string>(); // Store enum as string (Low, Medium, etc.) for readable DB values

            builder.Property(t => t.Order)
                .IsRequired();

            // One-to-many: a task belongs to one column
            builder.HasOne(t => t.Column)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.NoAction);

            // One-to-many: a task can be assigned to one user (nullable)
            builder.HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull); // If a user is deleted, keep the task but clear the assignee
        }
    }
}