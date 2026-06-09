using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventEase_st10157545_POE.Migrations
{
    /// <inheritdoc />
    public partial class Part3_EventTypeAndVenueAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventTypeID",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeID);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeID", "Description", "IsActive", "TypeName" },
                values: new object[,]
                {
                    { 1, "Wedding ceremonies and receptions", true, "Wedding" },
                    { 2, "Business meetings, conferences and seminars", true, "Corporate Conference" },
                    { 3, "Birthday celebrations of all ages", true, "Birthday Party" },
                    { 4, "Product reveals and brand launch events", true, "Product Launch" },
                    { 5, "Formal dinner and awards ceremonies", true, "Gala Dinner" },
                    { 6, "Educational sessions and skills workshops", true, "Workshop / Training" },
                    { 7, "Trade shows and public exhibitions", true, "Exhibition" },
                    { 8, "Live music, theatre and entertainment shows", true, "Concert / Show" },
                    { 9, "Sporting competitions and tournaments", true, "Sports Event" },
                    { 10, "Private gatherings and family functions", true, "Private Function" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_EventTypeID",
                table: "Event",
                column: "EventTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_EventTypes_EventTypeID",
                table: "Event",
                column: "EventTypeID",
                principalTable: "EventTypes",
                principalColumn: "EventTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_EventTypes_EventTypeID",
                table: "Event");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Event_EventTypeID",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EventTypeID",
                table: "Event");
        }
    }
}
