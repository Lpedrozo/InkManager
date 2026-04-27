using System.Security.Claims;
using System.Text;
using InkManager.Core.DTOs;
using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            // Buscar usuario por email o teléfono
            var user = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .Include(u => u.EstudioUsuarios)
                .ThenInclude(eu => eu.Estudio)
                .FirstOrDefaultAsync(u => (u.Email == dto.Email || u.Telefono == dto.Email) && u.Activo && !u.EliminadoLogico);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Correo electrónico o contraseña incorrectos"
                };
            }

            // Verificar contraseña
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
                RolEnEstudio = eu.RolEnEstudio ?? string.Empty,
                EsPrincipal = eu.EsPrincipal,
                HorarioLaboral = eu.HorarioLaboral
            }).ToList();

            // Obtener artistas asistidos (si es asistente)
            var artistasAsistidos = new List<ArtistaAsistidoDto>();
            if (roles.Any(r => r.Nombre == "asistente"))
            {
                artistasAsistidos = await _context.Asistentes
                    .Include(a => a.ArtistaAsistido)
                    .Include(a => a.Estudio)
                    .Where(a => a.UsuarioId == user.Id && a.Activo)
                    .Select(a => new ArtistaAsistidoDto
                    {
                        ArtistaId = a.ArtistaAsistidoId,
                        Nombre = a.ArtistaAsistido.Nombre,
                        EstudioId = a.EstudioId,
                        EstudioNombre = a.Estudio.Nombre
                    })
                    .ToListAsync();
            }

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
                Email = user.Email ?? string.Empty,
                Telefono = user.Telefono,
                Nombre = user.Nombre,
                Roles = roles.Select(r => r.Nombre).ToList(),
                FotoPerfilUrl = user.FotoPerfilUrl,
                Estudios = estudios
            };

            // Crear claims para el usuario
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim("Telefono", user.Telefono ?? "")
            };

            // Agregar roles como claims
            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
            }

            // Caso 1: Artista con un solo estudio → sesión directa
            if (roles.Any(r => r.Nombre == "artista") && estudios.Count == 1 && artistasAsistidos.Count == 0)
            {
                claims.Add(new Claim("EstudioId", estudios[0].EstudioId.ToString()));
                claims.Add(new Claim("EstudioNombre", estudios[0].Nombre));
                claims.Add(new Claim("RolSeleccionado", "artista"));

                user.UltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = true,
                    User = userInfo,
                    Claims = claims,
                    Message = "Login exitoso"
                };
            }

            // Caso 2: Artista con múltiples estudios → necesita seleccionar
            if (roles.Any(r => r.Nombre == "artista") && estudios.Count > 1)
            {
                return new LoginResponseDto
                {
                    Success = true,
                    User = userInfo,
                    AvailableRoles = roles.Where(r => r.Nombre == "artista").ToList(),
                    AvailableEstudios = estudios,
                    Message = "Selecciona el estudio donde trabajarás"
                };
            }

            // Caso 3: Asistente → necesita seleccionar artista
            if (roles.Any(r => r.Nombre == "asistente") && artistasAsistidos.Any())
            {
                return new LoginResponseDto
                {
                    Success = true,
                    User = userInfo,
                    AvailableRoles = roles.Where(r => r.Nombre == "asistente").ToList(),
                    ArtistasAsistidos = artistasAsistidos,
                    Message = "Selecciona el artista al que asistirás"
                };
            }

            // Caso 4: Admin o cliente con un solo rol
            if (roles.Count == 1)
            {
                claims.Add(new Claim("RolSeleccionado", roles[0].Nombre));

                if (estudios.Any())
                {
                    claims.Add(new Claim("EstudioId", estudios[0].EstudioId.ToString()));
                    claims.Add(new Claim("EstudioNombre", estudios[0].Nombre));
                }

                user.UltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new LoginResponseDto
                {
                    Success = true,
                    User = userInfo,
                    Claims = claims,
                    Message = "Login exitoso"
                };
            }

            // Caso 5: Múltiples opciones
            return new LoginResponseDto
            {
                Success = true,
                User = userInfo,
                AvailableRoles = roles,
                AvailableEstudios = estudios,
                ArtistasAsistidos = artistasAsistidos,
                Message = "Selecciona cómo quieres acceder"
            };
        }

        public async Task<LoginResponseDto> SelectRolAndEstudioAsync(SelectRolEstudioDto dto)
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

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, rol.Rol.Nombre),
                new Claim("Telefono", user.Telefono ?? ""),
                new Claim("RolSeleccionado", rol.Rol.Nombre)
            };

            // Si es artista, necesita estudio
            if (rol.Rol.Nombre == "artista" && dto.EstudioId.HasValue)
            {
                var estudio = user.EstudioUsuarios.FirstOrDefault(eu => eu.EstudioId == dto.EstudioId.Value);
                if (estudio == null)
                    throw new Exception("Estudio no válido para este artista");

                claims.Add(new Claim("EstudioId", dto.EstudioId.Value.ToString()));
                claims.Add(new Claim("EstudioNombre", estudio.Estudio.Nombre));
            }

            // Si es asistente, necesita artista
            if (rol.Rol.Nombre == "asistente" && dto.ArtistaId.HasValue)
            {
                var asistente = await _context.Asistentes
                    .Include(a => a.ArtistaAsistido)
                    .FirstOrDefaultAsync(a => a.UsuarioId == user.Id && a.ArtistaAsistidoId == dto.ArtistaId.Value && a.Activo);

                if (asistente == null)
                    throw new Exception("Artista no válido para este asistente");

                claims.Add(new Claim("ArtistaId", dto.ArtistaId.Value.ToString()));
                claims.Add(new Claim("ArtistaNombre", asistente.ArtistaAsistido.Nombre));
                claims.Add(new Claim("EstudioId", asistente.EstudioId.ToString()));
                claims.Add(new Claim("EstudioNombre", asistente.Estudio?.Nombre ?? ""));
            }

            // Actualizar último acceso
            user.UltimoAcceso = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new LoginResponseDto
            {
                Success = true,
                User = new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Telefono = user.Telefono,
                    Nombre = user.Nombre
                },
                Claims = claims,
                Message = "Sesión iniciada correctamente"
            };
        }
    }
}