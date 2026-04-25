using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ManyToManyEstudioUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Artistas_ArtistaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Asistentes_AsistenteId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Clientes_ClienteId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cubiculos_Artistas_ArtistaAsignadoId",
                table: "Cubiculos");

            migrationBuilder.DropForeignKey(
                name: "FK_MetricasDiarias_Artistas_ArtistaId",
                table: "MetricasDiarias");

            migrationBuilder.DropTable(
                name: "Asistentes");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Artistas");

            migrationBuilder.DropIndex(
                name: "IX_Citas_AsistenteId",
                table: "Citas");

            migrationBuilder.DropIndex(
                name: "IX_Citas_FechaHoraInicio",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "AsistenteId",
                table: "Citas");

            migrationBuilder.RenameColumn(
                name: "ArtistaId",
                table: "MetricasDiarias",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_MetricasDiarias_ArtistaId",
                table: "MetricasDiarias",
                newName: "IX_MetricasDiarias_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "ArtistaAsignadoId",
                table: "Cubiculos",
                newName: "UsuarioAsignadoId");

            migrationBuilder.RenameIndex(
                name: "IX_Cubiculos_ArtistaAsignadoId",
                table: "Cubiculos",
                newName: "IX_Cubiculos_UsuarioAsignadoId");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "Citas",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "ArtistaId",
                table: "Citas",
                newName: "ArtistaReferenciaId");

            migrationBuilder.RenameIndex(
                name: "IX_Citas_ClienteId",
                table: "Citas",
                newName: "IX_Citas_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Citas_ArtistaId_Estado",
                table: "Citas",
                newName: "IX_Citas_ArtistaReferenciaId_Estado");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaModificacion",
                table: "Citas",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Citas",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FotoPerfilUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstudioUsuarios",
                columns: table => new
                {
                    EstudioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RolEnEstudio = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstudioUsuarios", x => new { x.EstudioId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_EstudioUsuarios_Estudios_EstudioId",
                        column: x => x.EstudioId,
                        principalTable: "Estudios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstudioUsuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_FechaHoraInicio",
                table: "Citas",
                column: "FechaHoraInicio")
                .Annotation("SqlServer:Include", new[] { "Estado", "ArtistaReferenciaId" });

            migrationBuilder.CreateIndex(
                name: "IX_EstudioUsuarios_UsuarioId",
                table: "EstudioUsuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RolId",
                table: "UsuarioRoles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Usuarios_ArtistaReferenciaId",
                table: "Citas",
                column: "ArtistaReferenciaId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Usuarios_UsuarioId",
                table: "Citas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cubiculos_Usuarios_UsuarioAsignadoId",
                table: "Cubiculos",
                column: "UsuarioAsignadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MetricasDiarias_Usuarios_UsuarioId",
                table: "MetricasDiarias",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Usuarios_ArtistaReferenciaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Usuarios_UsuarioId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Cubiculos_Usuarios_UsuarioAsignadoId",
                table: "Cubiculos");

            migrationBuilder.DropForeignKey(
                name: "FK_MetricasDiarias_Usuarios_UsuarioId",
                table: "MetricasDiarias");

            migrationBuilder.DropTable(
                name: "EstudioUsuarios");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Citas_FechaHoraInicio",
                table: "Citas");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "MetricasDiarias",
                newName: "ArtistaId");

            migrationBuilder.RenameIndex(
                name: "IX_MetricasDiarias_UsuarioId",
                table: "MetricasDiarias",
                newName: "IX_MetricasDiarias_ArtistaId");

            migrationBuilder.RenameColumn(
                name: "UsuarioAsignadoId",
                table: "Cubiculos",
                newName: "ArtistaAsignadoId");

            migrationBuilder.RenameIndex(
                name: "IX_Cubiculos_UsuarioAsignadoId",
                table: "Cubiculos",
                newName: "IX_Cubiculos_ArtistaAsignadoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Citas",
                newName: "ClienteId");

            migrationBuilder.RenameColumn(
                name: "ArtistaReferenciaId",
                table: "Citas",
                newName: "ArtistaId");

            migrationBuilder.RenameIndex(
                name: "IX_Citas_UsuarioId",
                table: "Citas",
                newName: "IX_Citas_ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_Citas_ArtistaReferenciaId_Estado",
                table: "Citas",
                newName: "IX_Citas_ArtistaId_Estado");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaModificacion",
                table: "Citas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCreacion",
                table: "Citas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "AsistenteId",
                table: "Citas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Artistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudioId = table.Column<int>(type: "int", nullable: true),
                    ComisionPorcentaje = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FotoPerfilUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artistas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artistas_Estudios_EstudioId",
                        column: x => x.EstudioId,
                        principalTable: "Estudios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlergiasConocidas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotasGenerales = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Asistentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermisosJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asistentes_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_AsistenteId",
                table: "Citas",
                column: "AsistenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_FechaHoraInicio",
                table: "Citas",
                column: "FechaHoraInicio")
                .Annotation("SqlServer:Include", new[] { "Estado", "ArtistaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Artistas_Email",
                table: "Artistas",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artistas_EstudioId",
                table: "Artistas",
                column: "EstudioId");

            migrationBuilder.CreateIndex(
                name: "IX_Asistentes_ArtistaId",
                table: "Asistentes",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Artistas_ArtistaId",
                table: "Citas",
                column: "ArtistaId",
                principalTable: "Artistas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Asistentes_AsistenteId",
                table: "Citas",
                column: "AsistenteId",
                principalTable: "Asistentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Clientes_ClienteId",
                table: "Citas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cubiculos_Artistas_ArtistaAsignadoId",
                table: "Cubiculos",
                column: "ArtistaAsignadoId",
                principalTable: "Artistas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MetricasDiarias_Artistas_ArtistaId",
                table: "MetricasDiarias",
                column: "ArtistaId",
                principalTable: "Artistas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
