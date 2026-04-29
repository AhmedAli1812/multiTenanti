using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakePatientIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Floors_FloorId1",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Patients_PatientId1",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_PatientId1",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_FloorId1",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "FloorId1",
                table: "Rooms");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Intakes_TenantId_Status",
                table: "Intakes",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Intakes_TenantId_Status",
                table: "Intakes");

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId1",
                table: "Visits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FloorId1",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PatientId1",
                table: "Visits",
                column: "PatientId1");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_FloorId1",
                table: "Rooms",
                column: "FloorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Floors_FloorId1",
                table: "Rooms",
                column: "FloorId1",
                principalTable: "Floors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Patients_PatientId1",
                table: "Visits",
                column: "PatientId1",
                principalTable: "Patients",
                principalColumn: "Id");
        }
    }
}
