using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoqooePlanner.Migrations
{
    /// <inheritdoc />
    public partial class JournalEntryTimeStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeStamp",
                table: "JournalEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("UPDATE JournalEntries SET TimeStamp = json_extract(Eventdata, '$.timestamp')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeStamp",
                table: "JournalEntries");
        }
    }
}
