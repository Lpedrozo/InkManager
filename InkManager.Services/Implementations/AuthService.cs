using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InkManager.Core.DTOs;
using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InkManager.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            // Buscar usuario

            var user = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .Include(u => u.EstudioUsuarios)
                .ThenInclude(eu => eu.Estudio)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Activo && !u.EliminadoLogico);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Correo electrónico o contraseña incorrectos"
                };
            }

            // Verificar contraseña (temporal - usar BCrypt en producción)
            var passwordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.Password));
            if (user.PasswordHash != passwordHash)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Correo electrónico o contraseña incorrectos"
                };
            }

            // Obtener roles
            var roles = user.UsuarioRoles.Select(ur => new RolInfoDto
            {
                RolId = ur.RolId,
                Nombre = ur.Rol.Nombre,
                Descripcion = ur.Rol.Descripcion ?? string.Empty
            }).ToList();

            // Obtener estudios
            var estudios = user.EstudioUsuarios.Select(eu => new EstudioInfoDto
            {
                EstudioId = eu.EstudioId,
                Nombre = eu.Estudio.Nombre,
                Direccion = eu.Estudio.Direccion,
                RolEnEstudio = eu.RolEnEstudio ?? string.Empty
            }).ToList();

            if (roles.Count == 0)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "El usuario no tiene roles asignados"
                };
            }

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email,
                Nombre = user.Nombre,
                Roles = roles.Select(r => r.Nombre).ToList(),
                FotoPerfilUrl = user.FotoPerfilUrl
            };

            // Si solo tiene un rol y un estudio, generar sesión directamente
            if (roles.Count == 1 && estudios.Count == 1)
            {
                var session = new SessionInfoDto
                {
                    UsuarioId = user.Id,
                    Nombre = user.Nombre,
                    Email = user.Email,
                    RolId = roles[0].RolId,
                    RolNombre = roles[0].Nombre,
                    EstudioId = estudios[0].EstudioId,
                    EstudioNombre = estudios[0].Nombre
                };

                var token = GenerateJwtToken(session);

                // Actualizar último acceso
                user.UltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = true,
                    Token = token,
                    User = userInfo,
                    Message = "Login exitoso"
                };
            }

            // Si tiene múltiples opciones
            return new LoginResponseDto
            {
                Success = true,
                User = userInfo,
                AvailableRoles = roles,
                AvailableEstudios = estudios,
                Message = "Selecciona el rol y estudio para continuar"
            };
        }

        public async Task<SessionInfoDto> SelectRolAndEstudioAsync(SelectRolEstudioDto dto)
        {
            var user = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .Include(u => u.EstudioUsuarios)
                .ThenInclude(eu => eu.Estudio)
                .FirstOrDefaultAsync(u => u.Id == dto.UsuarioId && u.Activo && !u.EliminadoLogico);

            if (user == null)
                throw new Exception("Usuario no encontrado");

            // Verificar que el rol pertenezca al usuario
            var rol = user.UsuarioRoles.FirstOrDefault(ur => ur.RolId == dto.RolId);
            if (rol == null)
                throw new Exception("Rol no válido para este usuario");

            // Verificar que el estudio pertenezca al usuario
            var estudio = user.EstudioUsuarios.FirstOrDefault(eu => eu.EstudioId == dto.EstudioId);
            if (estudio == null)
                throw new Exception("Estudio no válido para este usuario");

            // Actualizar último acceso
            user.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new SessionInfoDto
            {
                UsuarioId = user.Id,
                Nombre = user.Nombre,
                Email = user.Email,
                RolId = dto.RolId,
                RolNombre = rol.Rol.Nombre,
                EstudioId = dto.EstudioId,
                EstudioNombre = estudio.Estudio.Nombre
            };
        }

        public string GenerateJwtToken(SessionInfoDto session)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, session.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, session.Email),
                new Claim(ClaimTypes.Name, session.Nombre),
                new Claim(ClaimTypes.Role, session.RolNombre),
                new Claim("EstudioId", session.EstudioId.ToString()),
                new Claim("EstudioNombre", session.EstudioNombre)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "InkManagerSecretKey2024VeryLongAndSecure!!!"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}