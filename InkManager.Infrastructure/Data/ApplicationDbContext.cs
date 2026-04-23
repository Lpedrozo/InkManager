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

        // DbSets
        public DbSet<Estudio> Estudios { get; set; }
        public DbSet<Artista> Artistas { get; set; }
        public DbSet<Asistente> Asistentes { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<ZonaCuerpo> ZonasCuerpo { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<PagoParcial> PagosParciales { get; set; }
        public DbSet<HistorialEstadoCita> HistorialEstadosCita { get; set; }
        public DbSet<Cubiculo> Cubiculos { get; set; }
        public DbSet<ConfiguracionCorreo> ConfiguracionesCorreo { get; set; }
        public DbSet<MetricaDiaria> MetricasDiarias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar soft delete global para entidades que lo soportan
            modelBuilder.Entity<Estudio>().HasQueryFilter(e => !e.EliminadoLogico);
            modelBuilder.Entity<Artista>().HasQueryFilter(a => !a.EliminadoLogico);
            modelBuilder.Entity<Asistente>().HasQueryFilter(a => !a.EliminadoLogico);
            modelBuilder.Entity<Cliente>().HasQueryFilter(c => !c.EliminadoLogico);
            modelBuilder.Entity<Cita>().HasQueryFilter(c => !c.EliminadoLogico);
            modelBuilder.Entity<Cubiculo>().HasQueryFilter(c => !c.EliminadoLogico);
            modelBuilder.Entity<ZonaCuerpo>().HasQueryFilter(z => !z.EliminadoLogico);

            // Configurar restricciones de eliminación (evitar eliminaciones en cascada peligrosas)

            // Estudio - Artista (restrict para evitar borrar estudio con artistas)
            modelBuilder.Entity<Artista>()
                .HasOne(a => a.Estudio)
                .WithMany(e => e.Artistas)
                .HasForeignKey(a => a.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Artista - Asistente (cascade está bien)
            modelBuilder.Entity<Asistente>()
                .HasOne(a => a.Artista)
                .WithMany(a => a.Asistentes)
                .HasForeignKey(a => a.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cita - Cliente (restrict)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Cliente)
                .WithMany(c => c.Citas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cita - Artista (restrict)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Artista)
                .WithMany(a => a.Citas)
                .HasForeignKey(c => c.ArtistaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cita - Asistente (set null)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Asistente)
                .WithMany(a => a.Citas)
                .HasForeignKey(c => c.AsistenteId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cita - ZonaCuerpo (set null)
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.ZonaCuerpo)
                .WithMany(z => z.Citas)
                .HasForeignKey(c => c.ZonaCuerpoId)
                .OnDelete(DeleteBehavior.SetNull);

            // PagoParcial - Cita (cascade está bien)
            modelBuilder.Entity<PagoParcial>()
                .HasOne(p => p.Cita)
                .WithMany(c => c.PagosParciales)
                .HasForeignKey(p => p.CitaId)
                .OnDelete(DeleteBehavior.Cascade);

            // HistorialEstadoCita - Cita (cascade)
            modelBuilder.Entity<HistorialEstadoCita>()
                .HasOne(h => h.Cita)
                .WithMany(c => c.HistorialEstados)
                .HasForeignKey(h => h.CitaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cubiculo - Estudio (cascade)
            modelBuilder.Entity<Cubiculo>()
                .HasOne(c => c.Estudio)
                .WithMany(e => e.Cubiculos)
                .HasForeignKey(c => c.EstudioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cubiculo - Artista (set null)
            modelBuilder.Entity<Cubiculo>()
                .HasOne(c => c.ArtistaAsignado)
                .WithMany(a => a.CubiculosAsignados)
                .HasForeignKey(c => c.ArtistaAsignadoId)
                .OnDelete(DeleteBehavior.SetNull);

            // ConfiguracionCorreo - Estudio (restrict)
            modelBuilder.Entity<ConfiguracionCorreo>()
                .HasOne(c => c.Estudio)
                .WithMany(e => e.ConfiguracionesCorreo)
                .HasForeignKey(c => c.EstudioId)
                .OnDelete(DeleteBehavior.Restrict);

            // MetricaDiaria - Estudio (cascade)
            modelBuilder.Entity<MetricaDiaria>()
                .HasOne(m => m.Estudio)
                .WithMany()
                .HasForeignKey(m => m.EstudioId)
                .OnDelete(DeleteBehavior.Cascade);

            // MetricaDiaria - Artista (cascade)
            modelBuilder.Entity<MetricaDiaria>()
                .HasOne(m => m.Artista)
                .WithMany()
                .HasForeignKey(m => m.ArtistaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para mejorar rendimiento
            modelBuilder.Entity<Cita>()
                .HasIndex(c => c.FechaHoraInicio)
                .IncludeProperties(c => new { c.Estado, c.ArtistaId });

            modelBuilder.Entity<Cita>()
                .HasIndex(c => new { c.ArtistaId, c.Estado });

            modelBuilder.Entity<Artista>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Estudio>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email);

            // Valores por defecto para fechas
            modelBuilder.Entity<Cita>()
                .Property(c => c.FechaCreacion)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Cita>()
                .Property(c => c.FechaModificacion)
                .HasDefaultValueSql("GETUTCDATE()");

            // Check constraints para estados
            modelBuilder.Entity<Cita>()
                .ToTable(t => t.HasCheckConstraint("CK_Cita_Estado",
                    "Estado IN ('pendiente', 'confirmada', 'en_curso', 'completada', 'cancelada', 'no_asistio')"));

            modelBuilder.Entity<PagoParcial>()
                .ToTable(t => t.HasCheckConstraint("CK_PagoParcial_MetodoPago",
                    "MetodoPago IN ('efectivo', 'tarjeta', 'transferencia', 'otro')"));
        }

        // Sobrescribir SaveChanges para actualizar FechaModificacion automáticamente
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