using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RescueLink.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Species = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    PetName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Breed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryColor = table.Column<int>(type: "int", nullable: false),
                    SecondaryColor = table.Column<int>(type: "int", nullable: true),
                    EventDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PetReportPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PetReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetReportPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetReportPhotos_PetReports_PetReportId",
                        column: x => x.PetReportId,
                        principalTable: "PetReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetReportPhotos_PetReportId_DisplayOrder",
                table: "PetReportPhotos",
                columns: new[] { "PetReportId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetReportPhotos_PetReportId_StorageKey",
                table: "PetReportPhotos",
                columns: new[] { "PetReportId", "StorageKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetReports_ReportType_Status_Species",
                table: "PetReports",
                columns: new[] { "ReportType", "Status", "Species" });

            migrationBuilder.CreateIndex(
                name: "IX_PetReports_UserId",
                table: "PetReports",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PetReportPhotos");

            migrationBuilder.DropTable(
                name: "PetReports");
        }
    }
}
