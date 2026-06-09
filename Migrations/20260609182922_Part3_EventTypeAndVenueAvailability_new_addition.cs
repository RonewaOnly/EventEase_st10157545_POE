using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase_st10157545_POE.Migrations
{
    /// <inheritdoc />
    public partial class Part3_EventTypeAndVenueAvailability_new_addition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Venue",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Venue");
        }
    }
}
