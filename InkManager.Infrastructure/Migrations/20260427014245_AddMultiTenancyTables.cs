using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "EstudioId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsPrincipal",
                table: "EstudioUsuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HorarioLaboral",
                table: "EstudioUsuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Asistentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ArtistaAsistidoId = table.Column<int>(type: "int", nullable: false),
                    EstudioId = table.Column<int>(type: "int", nullable: false),
                    Permisos = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistentes_Estudios_EstudioId",
                        column: x => x.EstudioId,
                        principalTable: "Estudios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistentes_Usuarios_ArtistaAsistidoId",
                        column: x => x.ArtistaAsistidoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asistentes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Calendarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    GoogleCalendarId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientesArtistas",
                columns: table => new
                {
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    EstudioId = table.Column<int>(type: "int", nullable: false),
                    FechaAsociacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesArtistas", x => new { x.ClienteId, x.ArtistaId });
                    table.ForeignKey(
                        name: "FK_ClientesArtistas_Estudios_EstudioId",
                        column: x => x.EstudioId,
                        principalTable: "Estudios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientesArtistas_Usuarios_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientesArtistas_Usuarios_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventosCalendario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CalendarioId = table.Column<int>(type: "int", nullable: false),
                    CitaId = table.Column<int>(type: "int", nullable: true),
                    TipoEvento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    GoogleEventId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosCalendario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosCalendario_Calendarios_CalendarioId",
                        column: x => x.CalendarioId,
                        principalTable: "Calendarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventosCalendario_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EstudioId",
                table: "Usuarios",
                column: "EstudioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Telefono",
                table: "Usuarios",
                column: "Telefono",
                unique: true,
                filter: "[Telefono] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Asistentes_ArtistaAsistidoId",
                table: "Asistentes",
                column: "ArtistaAsistidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistentes_EstudioId",
                table: "Asistentes",
                column: "EstudioId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistentes_UsuarioId",
                table: "Asistentes",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendarios_Tipo_EntidadId",
                table: "Calendarios",
                columns: new[] { "Tipo", "EntidadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Calendarios_UsuarioId",
                table: "Calendarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesArtistas_ArtistaId",
                table: "ClientesArtistas",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesArtistas_EstudioId",
                table: "ClientesArtistas",
                column: "EstudioId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosCalendario_CalendarioId",
                table: "EventosCalendario",
                column: "CalendarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosCalendario_CitaId",
                table: "EventosCalendario",
                column: "CitaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Estudios_EstudioId",
                table: "Usuarios",
                column: "EstudioId",
                principalTable: "Estudios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Estudios_EstudioId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Asistentes");

            migrationBuilder.DropTable(
                name: "ClientesArtistas");

            migrationBuilder.DropTable(
                name: "EventosCalendario");

            migrationBuilder.DropTable(
                name: "Calendarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EstudioId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Telefono",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EstudioId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EsPrincipal",
                table: "EstudioUsuarios");

            migrationBuilder.DropColumn(
                name: "HorarioLaboral",
                table: "EstudioUsuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }
    }
}
