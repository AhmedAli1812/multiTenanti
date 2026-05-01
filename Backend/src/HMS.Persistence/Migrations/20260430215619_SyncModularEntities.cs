using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModularEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PayerType",
                schema: "visits",
                table: "Visits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ArrivalMethod",
                schema: "visits",
                table: "Visits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChiefComplaint",
                schema: "visits",
                table: "Visits",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "visits",
                table: "Visits",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                schema: "visits",
                table: "Visits",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "intake",
                table: "Intakes",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalMethod",
                schema: "visits",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "ChiefComplaint",
                schema: "visits",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "visits",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "visits",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "intake",
                table: "Intakes");

            migrationBuilder.AlterColumn<int>(
                name: "PayerType",
                schema: "visits",
                table: "Visits",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
