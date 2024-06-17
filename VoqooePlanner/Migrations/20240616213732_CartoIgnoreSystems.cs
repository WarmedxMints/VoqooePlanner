using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoqooePlanner.Migrations
{
    /// <inheritdoc />
    public partial class CartoIgnoreSystems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartoIgnoredSystems",
                columns: table => new
                {
                    Address = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartoIgnoredSystems", x => x.Address);
                });

            migrationBuilder.CreateTable(
                name: "CommanderIgnoredSystems",
                columns: table => new
                {
                    CartoIgnoredSystemsDTOAddress = table.Column<long>(type: "INTEGER", nullable: false),
                    CommandersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommanderIgnoredSystems", x => new { x.CartoIgnoredSystemsDTOAddress, x.CommandersId });
                    table.ForeignKey(
                        name: "FK_CommanderIgnoredSystems_CartoIgnoredSystems_CartoIgnoredSystemsDTOAddress",
                        column: x => x.CartoIgnoredSystemsDTOAddress,
                        principalTable: "CartoIgnoredSystems",
                        principalColumn: "Address",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommanderIgnoredSystems_JournalCommanders_CommandersId",
                        column: x => x.CommandersId,
                        principalTable: "JournalCommanders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommanderIgnoredSystems_CommandersId",
                table: "CommanderIgnoredSystems",
                column: "CommandersId");

            //Fix my spelling mistake :P 
            migrationBuilder.Sql("DELETE FROM Settings WHERE Id = 'CatrographicAge'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommanderIgnoredSystems");

            migrationBuilder.DropTable(
                name: "CartoIgnoredSystems");
        }
    }
}
