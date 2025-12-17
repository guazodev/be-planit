using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanIT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTravelAndGoogleAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGenerated",
                table: "Travels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItineraryJson",
                table: "Travels",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGenerated",
                table: "Travels");

            migrationBuilder.DropColumn(
                name: "ItineraryJson",
                table: "Travels");
        }
    }
}
