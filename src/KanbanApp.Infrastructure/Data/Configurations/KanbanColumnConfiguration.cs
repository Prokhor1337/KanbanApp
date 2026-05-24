using KanbanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanApp.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Database configuration for the KanbanColumn entity using Fluent API.
    /// </summary>
    public class KanbanColumnConfiguration : IEntityTypeConfiguration<KanbanColumn>
    {
        public void Configure(EntityTypeBuilder<KanbanColumn> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Order)
                .IsRequired();

            // One-to-many: a column belongs to one board
            builder.HasOne(c => c.Board)
                .WithMany(b => b.Columns)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}