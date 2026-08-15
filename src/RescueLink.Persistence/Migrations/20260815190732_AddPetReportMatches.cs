using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueLink.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetReportMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetReportMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LostReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FoundReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    DistanceMeters = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetReportMatches", x => x.Id);
                    table.CheckConstraint("CK_PetReportMatches_DistanceMeters", "[DistanceMeters] >= 0");
                    table.CheckConstraint("CK_PetReportMatches_Score", "[Score] >= 0 AND [Score] <= 100");
                    table.ForeignKey(
                        name: "FK_PetReportMatches_PetReports_FoundReportId",
                        column: x => x.FoundReportId,
                        principalTable: "PetReports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PetReportMatches_PetReports_LostReportId",
                        column: x => x.LostReportId,
                        principalTable: "PetReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PetReportMatches_FoundReportId",
                table: "PetReportMatches",
                column: "FoundReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PetReportMatches_LostReportId_FoundReportId",
                table: "PetReportMatches",
                columns: new[] { "LostReportId", "FoundReportId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PetReportMatches_Status_Score",
                table: "PetReportMatches",
                columns: new[] { "Status", "Score" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PetReportMatches");
        }
    }
}
