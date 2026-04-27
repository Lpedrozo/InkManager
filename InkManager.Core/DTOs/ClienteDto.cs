using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string? FotoPerfilUrl { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int TotalCitas { get; set; }
        public decimal TotalGastado { get; set; }
    }

    public class CrearClienteDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [MaxLength(20)]
        [MinLength(6, ErrorMessage = "El teléfono debe tener al menos 6 caracteres")]
        public string Telefono { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string? Password { get; set; }  
        public string? Notas { get; set; } 
    }

    public class ActualizarClienteDto
    {
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? FotoPerfilUrl { get; set; }
    }
    public class CrearClienteRapidoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}