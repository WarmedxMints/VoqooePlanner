using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoqooePlanner.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JournalCommanders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    JournalDir = table.Column<string>(type: "TEXT", nullable: false),
                    LastFile = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalCommanders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Filename = table.Column<string>(type: "TEXT", nullable: false),
                    Offset = table.Column<long>(type: "INTEGER", nullable: false),
                    CommanderID = table.Column<int>(type: "INTEGER", nullable: false),
                    EventTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventData = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => new { x.Filename, x.Offset });
                });

            migrationBuilder.CreateTable(
                name: "Systems",
                columns: table => new
                {
                    Address = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Z = table.Column<double>(type: "REAL", nullable: false),
                    Visited = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContainsELW = table.Column<bool>(type: "INTEGER", nullable: false),
                    StarType = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Systems", x => x.Address);
                });

            migrationBuilder.CreateTable(
                name: "CommanderVistedSystems",
                columns: table => new
                {
                    CommanderVisitsId = table.Column<int>(type: "INTEGER", nullable: false),
                    VoqooeSystemDTOAddress = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommanderVistedSystems", x => new { x.CommanderVisitsId, x.VoqooeSystemDTOAddress });
                    table.ForeignKey(
                        name: "FK_CommanderVistedSystems_JournalCommanders_CommanderVisitsId",
                        column: x => x.CommanderVisitsId,
                        principalTable: "JournalCommanders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommanderVistedSystems_Systems_VoqooeSystemDTOAddress",
                        column: x => x.VoqooeSystemDTOAddress,
                        principalTable: "Systems",
                        principalColumn: "Address",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommanderVistedSystems_VoqooeSystemDTOAddress",
                table: "CommanderVistedSystems",
                column: "VoqooeSystemDTOAddress");

            migrationBuilder.CreateIndex(
                name: "IX_JournalCommanders_Name",
                table: "JournalCommanders",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommanderVistedSystems");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "JournalCommanders");

            migrationBuilder.DropTable(
                name: "Systems");
        }
    }
}
