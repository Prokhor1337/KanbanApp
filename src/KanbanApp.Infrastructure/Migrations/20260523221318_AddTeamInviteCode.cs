using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KanbanApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamInviteCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InviteCode",
                table: "Teams",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Teams");
        }
    }
}
