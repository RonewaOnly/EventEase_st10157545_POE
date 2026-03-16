using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase_st10157545_POE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingSpecialist",
                columns: table => new
                {
                    SpecialistID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingSpecialist", x => x.SpecialistID);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.CustomerID);
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpectedGuests = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.EventID);
                });

            migrationBuilder.CreateTable(
                name: "Venue",
                columns: table => new
                {
                    VenueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    PricePerDay = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venue", x => x.VenueID);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialistID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TablesAffected = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_AuditLog_BookingSpecialist_SpecialistID",
                        column: x => x.SpecialistID,
                        principalTable: "BookingSpecialist",
                        principalColumn: "SpecialistID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    VenueID = table.Column<int>(type: "int", nullable: false),
                    EventID = table.Column<int>(type: "int", nullable: false),
                    SpecialistID = table.Column<int>(type: "int", nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingID);
                    table.ForeignKey(
                        name: "FK_Bookings_BookingSpecialist_SpecialistID",
                        column: x => x.SpecialistID,
                        principalTable: "BookingSpecialist",
                        principalColumn: "SpecialistID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Customer_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customer",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Event_EventID",
                        column: x => x.EventID,
                        principalTable: "Event",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookings_Venue_VenueID",
                        column: x => x.VenueID,
                        principalTable: "Venue",
                        principalColumn: "VenueID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VenueSchedules",
                columns: table => new
                {
                    ScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueID = table.Column<int>(type: "int", nullable: false),
                    BookingID = table.Column<int>(type: "int", nullable: true),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    isBlocked = table.Column<bool>(type: "bit", nullable: false),
                    BlockReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueSchedules", x => x.ScheduleID);
                    table.ForeignKey(
                        name: "FK_VenueSchedules_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VenueSchedules_Venue_VenueID",
                        column: x => x.VenueID,
                        principalTable: "Venue",
                        principalColumn: "VenueID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BookingSpecialist",
                columns: new[] { "SpecialistID", "Email", "FirstName", "LastName", "Password", "Role" },
                values: new object[] { 1, "admin@eventease.co.za", "Admin", "User", "$2b$11$zWmUQTosMUfdINFVTppjbOmy6W5kuhO9CyP6Sc4sZzCsA3trwcO0y", "Admin" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_SpecialistID",
                table: "AuditLog",
                column: "SpecialistID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerID",
                table: "Bookings",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventID",
                table: "Bookings",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SpecialistID",
                table: "Bookings",
                column: "SpecialistID");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VenueID",
                table: "Bookings",
                column: "VenueID");

            migrationBuilder.CreateIndex(
                name: "IX_VenueSchedules_BookingID",
                table: "VenueSchedules",
                column: "BookingID",
                unique: true,
                filter: "[BookingID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VenueSchedules_VenueID",
                table: "VenueSchedules",
                column: "VenueID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "VenueSchedules");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "BookingSpecialist");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Venue");
        }
    }
}
