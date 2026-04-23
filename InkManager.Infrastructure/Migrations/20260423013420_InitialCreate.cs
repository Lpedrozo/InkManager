using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AlergiasConocidas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotasGenerales = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estudios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfiguracionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZonasCuerpo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CoordenadasJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrdenVisual = table.Column<int>(type: "int", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZonasCuerpo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Artistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FotoPerfilUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ComisionPorcentaje = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstudioId = table.Column<int>(type: "int", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "ConfiguracionesCorreo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmtpServer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Puerto = table.Column<int>(type: "int", nullable: false),
                    UsarSSL = table.Column<bool>(type: "bit", nullable: false),
                    EmailEnvio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordEncriptada = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HorarioRecordatorio = table.Column<TimeSpan>(type: "time", nullable: false),
                    DiasAntelacionRecordatorio = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EstudioId = table.Column<int>(type: "int", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesCorreo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesCorreo_Estudios_EstudioId",
                        column: x => x.EstudioId,
                        principalTable: "Estudios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Asistentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermisosJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Cubiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PosicionX = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PosicionY = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PosicionZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ancho = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Largo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Alto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    EstudioId = table.Column<int>(type: "int", nullable: false),
                    ArtistaAsignadoId = table.Column<int>(type: "int", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cubiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cubiculos_Artistas_ArtistaAsignadoId",
                        column: x => x.ArtistaAsignadoId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cubiculos_Estudios_EstudioId",
                        column: x => x.EstudioId,
                        principalTable: "Estudios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetricasDiarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCitasCompletadas = table.Column<int>(type: "int", nullable: false),
                    TotalIngresos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalHorasTrabajadas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstudioId = table.Column<int>(type: "int", nullable: false),
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricasDiarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetricasDiarias_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetricasDiarias_Estudios_EstudioId",
                        column: x => x.EstudioId,
                        principalTable: "Estudios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaHoraInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Adelanto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TamanioCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NotasInternas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotasPublicas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FotoReferenciaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiereRecordatorio = table.Column<bool>(type: "bit", nullable: false),
                    FechaRecordatorioEnviado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ArtistaId = table.Column<int>(type: "int", nullable: false),
                    AsistenteId = table.Column<int>(type: "int", nullable: true),
                    ZonaCuerpoId = table.Column<int>(type: "int", nullable: true),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                    table.CheckConstraint("CK_Cita_Estado", "Estado IN ('pendiente', 'confirmada', 'en_curso', 'completada', 'cancelada', 'no_asistio')");
                    table.ForeignKey(
                        name: "FK_Citas_Artistas_ArtistaId",
                        column: x => x.ArtistaId,
                        principalTable: "Artistas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_Asistentes_AsistenteId",
                        column: x => x.AsistenteId,
                        principalTable: "Asistentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Citas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_ZonasCuerpo_ZonaCuerpoId",
                        column: x => x.ZonaCuerpoId,
                        principalTable: "ZonasCuerpo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HistorialEstadosCita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EstadoNuevo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioTipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CitaId = table.Column<int>(type: "int", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadosCita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEstadosCita_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagosParciales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferenciaPago = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Nota = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CitaId = table.Column<int>(type: "int", nullable: false),
                    EliminadoLogico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosParciales", x => x.Id);
                    table.CheckConstraint("CK_PagoParcial_MetodoPago", "MetodoPago IN ('efectivo', 'tarjeta', 'transferencia', 'otro')");
                    table.ForeignKey(
                        name: "FK_PagosParciales_Citas_CitaId",
                        column: x => x.CitaId,
                        principalTable: "Citas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Citas_ArtistaId_Estado",
                table: "Citas",
                columns: new[] { "ArtistaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_AsistenteId",
                table: "Citas",
                column: "AsistenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_ClienteId",
                table: "Citas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_FechaHoraInicio",
                table: "Citas",
                column: "FechaHoraInicio")
                .Annotation("SqlServer:Include", new[] { "Estado", "ArtistaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_ZonaCuerpoId",
                table: "Citas",
                column: "ZonaCuerpoId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesCorreo_EstudioId",
                table: "ConfiguracionesCorreo",
                column: "EstudioId");

            migrationBuilder.CreateIndex(
                name: "IX_Cubiculos_ArtistaAsignadoId",
                table: "Cubiculos",
                column: "ArtistaAsignadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cubiculos_EstudioId",
                table: "Cubiculos",
                column: "EstudioId");

            migrationBuilder.CreateIndex(
                name: "IX_Estudios_Email",
                table: "Estudios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosCita_CitaId",
                table: "HistorialEstadosCita",
                column: "CitaId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricasDiarias_ArtistaId",
                table: "MetricasDiarias",
                column: "ArtistaId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricasDiarias_EstudioId",
                table: "MetricasDiarias",
                column: "EstudioId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosParciales_CitaId",
                table: "PagosParciales",
                column: "CitaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesCorreo");

            migrationBuilder.DropTable(
                name: "Cubiculos");

            migrationBuilder.DropTable(
                name: "HistorialEstadosCita");

            migrationBuilder.DropTable(
                name: "MetricasDiarias");

            migrationBuilder.DropTable(
                name: "PagosParciales");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Asistentes");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "ZonasCuerpo");

            migrationBuilder.DropTable(
                name: "Artistas");

            migrationBuilder.DropTable(
                name: "Estudios");
        }
    }
}
