using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InkManager.Core.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true; // Por defecto 7 días
    }
    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string? FotoPerfilUrl { get; set; }
        public List<EstudioInfoDto>? Estudios { get; set; }
    }
    public class UserRolesDto
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RolInfoDto> Roles { get; set; } = new();
    }

    public class RolInfoDto
    {
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class EstudioInfoDto
    {
        public int EstudioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? RolEnEstudio { get; set; }
        public bool EsPrincipal { get; set; }
        public string? HorarioLaboral { get; set; }
    }

    public class ArtistaAsistidoDto
    {
        public int ArtistaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int EstudioId { get; set; }
        public string EstudioNombre { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? RedirectUrl { get; set; }
        public UserInfoDto? User { get; set; }
        public List<Claim>? Claims { get; set; }
        public List<RolInfoDto>? AvailableRoles { get; set; }
        public List<EstudioInfoDto>? AvailableEstudios { get; set; }
        public List<ArtistaAsistidoDto>? ArtistasAsistidos { get; set; }
    }
    public class SelectRolEstudioDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int RolId { get; set; }

        public int? EstudioId { get; set; }

        public int? ArtistaId { get; set; }

        public bool RememberMe { get; set; } = true;
    }
    public class SessionInfoDto
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public int? EstudioId { get; set; }
        public string? EstudioNombre { get; set; }
        public int? ArtistaId { get; set; }
        public string? ArtistaNombre { get; set; }
        public DateTime Expira { get; set; }
    }
}