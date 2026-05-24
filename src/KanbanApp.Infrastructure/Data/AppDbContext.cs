using KanbanApp.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace KanbanApp.Infrastructure.Data
{
    /// Database context for the Kanban application, managing core entities and Identity tables.
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Board> Boards => Set<Board>();
        public DbSet<KanbanColumn> Columns => Set<KanbanColumn>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<TeamBoard> TeamBoards => Set<TeamBoard>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Automatically applies all configuration classes that implement IEntityTypeConfiguration from this assembly.
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}