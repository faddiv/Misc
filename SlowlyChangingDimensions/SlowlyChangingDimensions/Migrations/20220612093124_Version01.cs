using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlowlyChangingDimensions.Migrations
{
    public partial class Version01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScdExampleTable1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrelationId = table.Column<int>(type: "int", nullable: false),
                    Data1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Data2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Data3 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PreviousVersionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScdExampleTable1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScdExampleTable1_ScdExampleTable1_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalTable: "ScdExampleTable1",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScdExampleTable2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrelationId = table.Column<int>(type: "int", nullable: false),
                    CreatedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrentVersion = table.Column<bool>(type: "bit", nullable: false),
                    Data1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Data2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Data3 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScdExampleTable2", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScdExampleTable1_PreviousVersionId",
                table: "ScdExampleTable1",
                column: "PreviousVersionId",
                unique: true,
                filter: "[PreviousVersionId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScdExampleTable1");

            migrationBuilder.DropTable(
                name: "ScdExampleTable2");
        }
    }
}
