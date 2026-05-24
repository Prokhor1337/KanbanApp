using KanbanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KanbanApp.Infrastructure.Data.Configurations
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
            builder.Property(t => t.Description).HasMaxLength(500);
            builder.Property(t => t.OwnerId).IsRequired();

            builder.HasOne(t => t.Owner)
                   .WithMany()
                   .HasForeignKey(t => t.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
    {
        public void Configure(EntityTypeBuilder<TeamMember> builder)
        {
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Role).IsRequired().HasMaxLength(20);

            builder.HasOne(m => m.Team)
                   .WithMany(t => t.Members)
                   .HasForeignKey(m => m.TeamId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.User)
                   .WithMany()
                   .HasForeignKey(m => m.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // One user per team
            builder.HasIndex(m => new { m.TeamId, m.UserId }).IsUnique();
        }
    }

    public class TeamBoardConfiguration : IEntityTypeConfiguration<TeamBoard>
    {
        public void Configure(EntityTypeBuilder<TeamBoard> builder)
        {
            // Composite PK — no separate Id needed
            builder.HasKey(tb => new { tb.TeamId, tb.BoardId });

            builder.HasOne(tb => tb.Team)
                   .WithMany(t => t.TeamBoards)
                   .HasForeignKey(tb => tb.TeamId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tb => tb.Board)
                   .WithMany()
                   .HasForeignKey(tb => tb.BoardId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
