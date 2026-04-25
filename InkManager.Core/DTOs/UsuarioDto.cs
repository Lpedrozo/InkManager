namespace InkManager.Core.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class TimeSlotDto
    {
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Disponible { get; set; }
        public string Display => $"{HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";
    }
}