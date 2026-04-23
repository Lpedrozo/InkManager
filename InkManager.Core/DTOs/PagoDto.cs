using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.DTOs
{
    public class PagoParcialDto
    {
        public int Id { get; set; }
        public int CitaId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ArtistaNombre { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? ReferenciaPago { get; set; }
        public string? Nota { get; set; }
        public string MetodoPagoDisplay => MetodoPago switch
        {
            "efectivo" => "💵 Efectivo",
            "tarjeta" => "💳 Tarjeta",
            "transferencia" => "🏦 Transferencia",
            "otro" => "📝 Otro",
            _ => MetodoPago
        };
    }

    public class RegistrarPagoDto
    {
        [Required]
        public int CitaId { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required]
        public string MetodoPago { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReferenciaPago { get; set; }

        [MaxLength(500)]
        public string? Nota { get; set; }
    }

    public class HistorialPagosDto
    {
        public int CitaId { get; set; }
        public decimal PrecioTotal { get; set; }
        public decimal Adelanto { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public List<PagoParcialDto> Pagos { get; set; } = new();
    }
}