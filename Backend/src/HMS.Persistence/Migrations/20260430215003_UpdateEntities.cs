using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Visits_PatientId",
                schema: "visits",
                table: "Visits",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                schema: "identity",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSessions_Users_UserId",
                schema: "identity",
                table: "UserSessions",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Patients_PatientId",
                schema: "visits",
                table: "Visits",
                column: "PatientId",
                principalSchema: "patients",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSessions_Users_UserId",
                schema: "identity",
                table: "UserSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Patients_PatientId",
                schema: "visits",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_PatientId",
                schema: "visits",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_UserId",
                schema: "identity",
                table: "UserSessions");
        }
    }
}
