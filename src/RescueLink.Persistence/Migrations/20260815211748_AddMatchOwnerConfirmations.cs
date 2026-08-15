using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueLink.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchOwnerConfirmations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FoundOwnerConfirmed",
                table: "PetReportMatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LostOwnerConfirmed",
                table: "PetReportMatches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoundOwnerConfirmed",
                table: "PetReportMatches");

            migrationBuilder.DropColumn(
                name: "LostOwnerConfirmed",
                table: "PetReportMatches");
        }
    }
}
