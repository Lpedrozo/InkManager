using System.Text;
using InkManager.Core.DTOs;
using InkManager.Core.DTOs.Common;
using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Services.Implementations
{
    public class ClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;

        public ClienteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClienteDto?> GetByIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .Include(u => u.CitasComoCliente)
                .FirstOrDefaultAsync(u => u.Id == id && u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente") && !u.EliminadoLogico);

            if (usuario == null) return null;

            return MapToDto(usuario);
        }

        public async Task<ClienteDto?> GetByTelefonoAsync(string telefono)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .Include(u => u.CitasComoCliente)
                .FirstOrDefaultAsync(u => u.Telefono == telefono && u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente") && !u.EliminadoLogico);

            if (usuario == null) return null;

            return MapToDto(usuario);
        }

        public async Task<ClienteDto> CreateAsync(CrearClienteDto dto)
        {
            // Verificar si ya existe por teléfono
            var existing = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Telefono == dto.Telefono && !u.EliminadoLogico);

            if (existing != null)
                throw new InvalidOperationException("Ya existe un cliente con este número de teléfono");

            // Verificar email único si se proporcionó
            if (!string.IsNullOrEmpty(dto.Email))
            {
                var emailExists = await _context.Usuarios
                    .AnyAsync(u => u.Email == dto.Email && !u.EliminadoLogico);
                if (emailExists)
                    throw new InvalidOperationException("Ya existe un usuario con este email");
            }

            // Contraseña genérica: "Cliente123!"
            var passwordGenerica = "Cliente123!";

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = string.IsNullOrEmpty(dto.Email) ? null : dto.Email,
                Telefono = dto.Telefono,
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(passwordGenerica)),
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                FechaModificacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Asignar rol de cliente
            var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "cliente");
            if (rolCliente != null)
            {
                _context.UsuarioRoles.Add(new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = rolCliente.Id
                });
                await _context.SaveChangesAsync();
            }

            return (await GetByIdAsync(usuario.Id))!;
        }
        public async Task<ClienteDto?> UpdateAsync(int id, ActualizarClienteDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .FirstOrDefaultAsync(u => u.Id == id && u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente") && !u.EliminadoLogico);

            if (usuario == null) return null;

            if (!string.IsNullOrEmpty(dto.Nombre))
                usuario.Nombre = dto.Nombre;

            if (dto.Email != null)
                usuario.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Telefono))
            {
                // Verificar que no exista otro con el mismo teléfono
                var exists = await _context.Usuarios
                    .AnyAsync(u => u.Telefono == dto.Telefono && u.Id != id && !u.EliminadoLogico);
                if (exists)
                    throw new InvalidOperationException("Ya existe otro cliente con este número de teléfono");
                usuario.Telefono = dto.Telefono;
            }

            if (dto.FotoPerfilUrl != null)
                usuario.FotoPerfilUrl = dto.FotoPerfilUrl;

            usuario.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente"));

            if (usuario == null) return false;

            // Verificar si tiene citas
            var tieneCitas = await _context.Citas.AnyAsync(c => c.UsuarioId == id && !c.EliminadoLogico);
            if (tieneCitas)
                throw new InvalidOperationException("No se puede eliminar un cliente con citas registradas");

            usuario.EliminadoLogico = true;
            usuario.FechaModificacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<ClienteDto>> GetAllAsync(int pagina = 1, int pageSize = 10, string? search = null)
        {
            try
            {
                var query = _context.Usuarios
                    .Include(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                    .Include(u => u.CitasComoCliente)
                    .Where(u => u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente") && !u.EliminadoLogico);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Nombre.Contains(search) ||
                                             u.Telefono.Contains(search) ||
                                             (u.Email != null && u.Email.Contains(search)));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(u => u.FechaCreacion)
                    .Skip((pagina - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<ClienteDto>
                {
                    Items = items.Select(MapToDto).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pagina,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                // Log del error
                Console.WriteLine($"Error en GetAllAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetTotalClientesAsync()
        {
            return await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .CountAsync(u => u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente") && !u.EliminadoLogico);
        }

        public async Task<List<ClienteDto>> GetClientesFrecuentesAsync(int top = 10)
        {
            var clientes = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .Include(u => u.CitasComoCliente)
                .Where(u => u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente") && !u.EliminadoLogico)
                .OrderByDescending(u => u.CitasComoCliente.Count)
                .Take(top)
                .ToListAsync();

            return clientes.Select(MapToDto).ToList();
        }
        public async Task<List<ClienteDto>> GetClientesNuevosPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var clientes = await _context.Usuarios
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .Include(u => u.CitasComoCliente)
                .Where(u => u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "cliente")
                    && !u.EliminadoLogico
                    && u.FechaCreacion >= fechaInicio
                    && u.FechaCreacion <= fechaFin)
                .OrderByDescending(u => u.FechaCreacion)
                .ToListAsync();

            return clientes.Select(MapToDto).ToList();
        }
        private ClienteDto MapToDto(Usuario usuario)
        {
            return new ClienteDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                FechaRegistro = usuario.FechaCreacion,
                TotalCitas = usuario.CitasComoCliente?.Count ?? 0,
                TotalGastado = usuario.CitasComoCliente?.Where(c => c.Estado == "completada").Sum(c => c.PrecioTotal) ?? 0
            };
        }
    }
}