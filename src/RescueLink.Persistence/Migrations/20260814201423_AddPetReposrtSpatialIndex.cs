using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RescueLink.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetReposrtSpatialIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        CREATE SPATIAL INDEX [IX_PetReports_Location]
        ON [dbo].[PetReports]([Location])
        USING GEOGRAPHY_AUTO_GRID
        WITH (CELLS_PER_OBJECT = 16);
        """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        DROP INDEX [IX_PetReports_Location]
        ON [dbo].[PetReports];
        """);
        }
    }
}
