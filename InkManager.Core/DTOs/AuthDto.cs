using System.ComponentModel.DataAnnotations;

namespace InkManager.Core.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string? FotoPerfilUrl { get; set; }
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
        public string RolEnEstudio { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public UserInfoDto? User { get; set; }
        public List<RolInfoDto>? AvailableRoles { get; set; }
        public List<EstudioInfoDto>? AvailableEstudios { get; set; }
    }

    public class SelectRolEstudioDto
    {
        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int RolId { get; set; }

        [Required]
        public int EstudioId { get; set; }
    }

    public class SessionInfoDto
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public int EstudioId { get; set; }
        public string EstudioNombre { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}