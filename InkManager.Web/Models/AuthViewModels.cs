namespace InkManager.Web.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class SelectRoleEstudioViewModel
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<RolOption> Roles { get; set; } = new();
        public List<EstudioOption> Estudios { get; set; } = new();
    }

    public class RolOption
    {
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Icono => Nombre switch
        {
            "admin" => "👑",
            "artista" => "🎨",
            "asistente" => "🤝",
            "cliente" => "👤",
            _ => "🔘"
        };
    }

    public class EstudioOption
    {
        public int EstudioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string RolEnEstudio { get; set; } = string.Empty;
    }
}