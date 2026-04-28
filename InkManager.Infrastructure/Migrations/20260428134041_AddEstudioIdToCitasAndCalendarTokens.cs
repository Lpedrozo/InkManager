using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEstudioIdToCitasAndCalendarTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstudioId",
                table: "Citas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "Calendarios",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "Calendarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSync",
                table: "Calendarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Calendarios",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiry",
                table: "Calendarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Citas_EstudioId",
                table: "Citas",
                column: "EstudioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Estudios_EstudioId",
                table: "Citas",
                column: "EstudioId",
                principalTable: "Estudios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Estudios_EstudioId",
                table: "Citas");

            migrationBuilder.DropIndex(
                name: "IX_Citas_EstudioId",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "EstudioId",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "Calendarios");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "Calendarios");

            migrationBuilder.DropColumn(
                name: "LastSync",
                table: "Calendarios");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Calendarios");

            migrationBuilder.DropColumn(
                name: "TokenExpiry",
                table: "Calendarios");
        }
    }
}
