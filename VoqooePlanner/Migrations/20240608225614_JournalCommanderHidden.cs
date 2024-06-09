using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoqooePlanner.Migrations
{
    /// <inheritdoc />
    public partial class JournalCommanderHidden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "JournalCommanders",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "JournalCommanders");
        }
    }
}
