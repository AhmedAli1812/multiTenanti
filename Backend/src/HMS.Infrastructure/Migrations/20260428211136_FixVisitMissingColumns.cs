using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixVisitMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArrivalMethod",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChiefComplaint",
                table: "Visits",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalMethod",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ChiefComplaint",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Visits");
        }
    }
}
