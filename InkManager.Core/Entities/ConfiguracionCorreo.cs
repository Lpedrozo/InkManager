using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.Entities
{
    public class ConfiguracionCorreo : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string SmtpServer { get; set; } = string.Empty;

        [Required]
        public int Puerto { get; set; } = 587;

        public bool UsarSSL { get; set; } = true;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string EmailEnvio { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordEncriptada { get; set; } = string.Empty;

        public TimeSpan HorarioRecordatorio { get; set; } = new TimeSpan(9, 0, 0);

        public int DiasAntelacionRecordatorio { get; set; } = 1;

        public bool Activo { get; set; } = true;

        // Foreign keys
        public int EstudioId { get; set; }

        // Navigation properties
        public virtual Estudio Estudio { get; set; } = null!;
    }
}