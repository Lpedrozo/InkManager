using InkManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets existentes
        public DbSet<Estudio> Estudios { get; set; }
        public DbSet<ZonaCuerpo> ZonasCuerpo { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<PagoParcial> PagosParciales { get; set; }
        public DbSet<HistorialEstadoCita> HistorialEstadosCita { get; set; }
        public DbSet<Cubiculo> Cubiculos { get; set; }
        public DbSet<ConfiguracionCorreo> ConfiguracionesCorreo { get; set; }
        public DbSet<MetricaDiaria> MetricasDiarias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioRol> UsuarioRoles { get; set; }
        public DbSet<EstudioUsuario> EstudioUsuarios { get; set; }

        // Nuevos DbSets
        public DbSet<Asistente> Asistentes { get; set; }
        public DbSet<ClienteArtista> ClientesArtistas { get; set; }
        public DbSet<Calendario> Calendarios { get; set; }
        public DbSet<EventoCalendario> EventosCalendario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // Soft delete filters
            // ============================================
            modelBuilder.Entity<Estudio>().HasQueryFilter(e => !e.EliminadoLogico);
            modelBuilder.Entity<Cita>().HasQueryFilter(c => !c.EliminadoLogico);
            modelBuilder.Entity<Cubiculo>().HasQueryFilter(c => !c.EliminadoLogico);
            modelBuilder.Entity<ZonaCuerpo>().HasQueryFilter(z => !z.EliminadoLogico);
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.EliminadoLogico);
            modelBuilder.Entity<Rol>().HasQueryFilter(r => !r.EliminadoLogico);
            modelBuilder.Entity<ConfiguracionCorreo>().HasQueryFilter(c => !c.EliminadoLogico);
            modelBuilder.Entity<MetricaDiaria>().HasQueryFilter(m => !m.EliminadoLogico);
            modelBuilder.Entity<Calendario>().HasQueryFilter(c => !c.EliminadoLogico);
            modelBuilder.Entity<EventoCalendario>().HasQueryFilter(e => !e.EliminadoLogico);

            // ============================================
            // Configuración de Usuario
            // ============================================
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Telefono)
                .IsUnique()
                .HasFilter("[Telefono] IS NOT NULL");

            // ============================================
            // Configuración de UsuarioRol
            // ============================================
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });

            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Usuario)
                .WithMany(u => u.UsuarioRoles)
                .HasForeignKey(ur => ur.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsuarioRol>()
                .HasOne(ur => ur.Rol)
                .WithMany(r => r.UsuarioRoles)
                .HasForeignKey(ur => ur.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // Configuración de EstudioUsuario
            // ============================================
            modelBuilder.Entity<EstudioUsuario>()
                .HasKey(eu => new { eu.EstudioId, eu.UsuarioId });

            modelBuilder.Entity<EstudioUsuario>()
                .HasOne(eu => eu.Estudio)
                .WithMany(e => e.EstudioUsuarios)
                .HasForeignKey(eu => eu.EstudioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EstudioUsuario>()
                .HasOne(eu => eu.Usuario)
                .WithMany(u => u.EstudioUsuarios)
                .HasForeignKey(eu => eu.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // Configuración de Asistente
            // ============================================
            modelBuilder.Entity<Asistente>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Cita>()
    .HasIndex(c => c.GoogleEventIdArtista)
    .IsUnique()
    .HasFilter("[GoogleEventIdArtista] IS NOT NULL");

            modelBuilder.Entity<Cita>()
                .HasIndex(c => c.GoogleEventIdEstudio)
                .IsUnique()
                .HasFilter("[GoogleEventIdEstudio] IS NOT NULL");
            modelBuilder.Entity<Asistente>()
                .HasOne(a => a.Usuario)
                .WithOne(u => u.AsistenteAsignado)
                .HasForeignKey<Asistente>(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Asistente>()
                .HasOne(a => a.ArtistaAsistido)
                .WithMany(u => u.AsistentesQueAyudan)
                .HasForeignKey(a => a.ArtistaAsistidoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asistente>()
                .HasOne(a => a.Estudio)
                .WithMany()
                .HasForeignKey(a => a.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // Configuración de ClienteArtista
            // ============================================
            modelBuilder.Entity<ClienteArtista>()
                .HasKey(ca => new { ca.ClienteId, ca.ArtistaId });

            modelBuilder.Entity<ClienteArtista>()
                .HasOne(ca => ca.Cliente)
                .WithMany(u => u.ArtistasQueAtienden)
                .HasForeignKey(ca => ca.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClienteArtista>()
                .HasOne(ca => ca.Artista)
                .WithMany(u => u.ClientesAtendidos)
                .HasForeignKey(ca => ca.ArtistaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClienteArtista>()
                .HasOne(ca => ca.Estudio)
                .WithMany()
                .HasForeignKey(ca => ca.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // Configuración de Calendario
            // ============================================
            modelBuilder.Entity<Calendario>()
                .HasIndex(c => new { c.Tipo, c.EntidadId })
                .IsUnique();

            // ============================================
            // Configuración de EventoCalendario
            // ============================================
            modelBuilder.Entity<EventoCalendario>()
                .HasOne(e => e.Calendario)
                .WithMany(c => c.Eventos)
                .HasForeignKey(e => e.CalendarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventoCalendario>()
                .HasOne(e => e.Cita)
                .WithMany()
                .HasForeignKey(e => e.CitaId)
                .OnDelete(DeleteBehavior.SetNull);

            // ============================================
            // Configuración de Cita con Usuario
            // ============================================
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.CitasComoCliente)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cita>()
                .HasOne(c => c.ArtistaReferencia)
                .WithMany(u => u.CitasComoArtista)
                .HasForeignKey(c => c.ArtistaReferenciaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // Configuración de Cita - ZonaCuerpo
            // ============================================
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.ZonaCuerpo)
                .WithMany(z => z.Citas)
                .HasForeignKey(c => c.ZonaCuerpoId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Estudio)
                .WithMany()
                .HasForeignKey(c => c.EstudioId)
                .OnDelete(DeleteBehavior.SetNull);
            // ============================================
            // Configuración de Pagos
            // ============================================
            modelBuilder.Entity<PagoParcial>()
                .HasOne(p => p.Cita)
                .WithMany(c => c.PagosParciales)
                .HasForeignKey(p => p.CitaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // Configuración de Historial de Estados
            // ============================================
            modelBuilder.Entity<HistorialEstadoCita>()
                .HasOne(h => h.Cita)
                .WithMany(c => c.HistorialEstados)
                .HasForeignKey(h => h.CitaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // Configuración de Cubiculos
            // ============================================
            modelBuilder.Entity<Cubiculo>()
                .HasOne(c => c.Estudio)
                .WithMany(e => e.Cubiculos)
                .HasForeignKey(c => c.EstudioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cubiculo>()
                .HasOne(c => c.UsuarioAsignado)
                .WithMany(u => u.CubiculosAsignados)
                .HasForeignKey(c => c.UsuarioAsignadoId)
                .OnDelete(DeleteBehavior.SetNull);

            // ============================================
            // Configuración de ConfiguracionCorreo
            // ============================================
            modelBuilder.Entity<ConfiguracionCorreo>()
                .HasOne(c => c.Estudio)
                .WithMany(e => e.ConfiguracionesCorreo)
                .HasForeignKey(c => c.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // Configuración de MetricasDiarias
            // ============================================
            modelBuilder.Entity<MetricaDiaria>()
                .HasOne(m => m.Estudio)
                .WithMany()
                .HasForeignKey(m => m.EstudioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MetricaDiaria>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MetricasDiarias)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================================
            // Índices para rendimiento
            // ============================================
            modelBuilder.Entity<Cita>()
                .HasIndex(c => c.FechaHoraInicio)
                .IncludeProperties(c => new { c.Estado, c.ArtistaReferenciaId });

            modelBuilder.Entity<Cita>()
                .HasIndex(c => new { c.ArtistaReferenciaId, c.Estado });

            modelBuilder.Entity<Cita>()
                .HasIndex(c => c.UsuarioId);

            modelBuilder.Entity<Estudio>()
                .HasIndex(e => e.Email)
                .IsUnique();

            // ============================================
            // Check constraints
            // ============================================
            modelBuilder.Entity<Cita>()
                .ToTable(t => t.HasCheckConstraint("CK_Cita_Estado",
                    "Estado IN ('pendiente', 'confirmada', 'en_curso', 'completada', 'cancelada', 'no_asistio')"));

            modelBuilder.Entity<PagoParcial>()
                .ToTable(t => t.HasCheckConstraint("CK_PagoParcial_MetodoPago",
                    "MetodoPago IN ('efectivo', 'tarjeta', 'transferencia', 'otro')"));
        }

        // ============================================
        // Timestamps automáticos
        // ============================================
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                           (e.State == EntityState.Modified || e.State == EntityState.Added));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                entity.FechaModificacion = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.FechaCreacion = DateTime.UtcNow;
                }
            }
        }
    }
}