using System.Security.Claims;
using System.Text;
using InkManager.Core.DTOs;
using InkManager.Core.DTOs.Common;
using InkManager.Core.Entities;
using InkManager.Infrastructure.Data;
using InkManager.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InkManager.Services.Implementations
{
    public class ClienteService : IClienteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClienteService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Obtener el ID del artista actual basado en el contexto del usuario
        private int GetArtistaIdActual()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return 0;

            var rol = user.FindFirst(ClaimTypes.Role)?.Value;
            var usuarioId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (rol == "artista")
            {
                return usuarioId;
            }
            else if (rol == "asistente")
            {
                var artistaId = user.FindFirst("ArtistaId")?.Value;
                return artistaId != null ? int.Parse(artistaId) : 0;
            }

            return 0;
        }

        public async Task<ClienteDto?> GetByIdAsync(int id)
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return null;

            // Buscar cliente que esté asociado al artista actual
            var clienteArtista = await _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.UsuarioRoles)
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.CitasComoCliente)
                .FirstOrDefaultAsync(ca => ca.ClienteId == id && ca.ArtistaId == artistaId);

            if (clienteArtista == null) return null;

            return MapToDto(clienteArtista.Cliente, clienteArtista);
        }

        public async Task<ClienteDto?> GetByTelefonoAsync(string telefono)
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return null;

            var clienteArtista = await _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.UsuarioRoles)
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.CitasComoCliente)
                .FirstOrDefaultAsync(ca => ca.Cliente.Telefono == telefono && ca.ArtistaId == artistaId);

            if (clienteArtista == null) return null;

            return MapToDto(clienteArtista.Cliente, clienteArtista);
        }

        public async Task<ClienteDto> CreateAsync(CrearClienteDto dto)
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) throw new InvalidOperationException("No se pudo identificar al artista");

            // Verificar si ya existe por teléfono
            var existing = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Telefono == dto.Telefono && !u.EliminadoLogico);

            Usuario usuario;
            bool esNuevoCliente = false;

            if (existing != null)
            {
                // El cliente ya existe, verificar si ya está asociado a este artista
                usuario = existing;
                var yaAsociado = await _context.ClientesArtistas
                    .AnyAsync(ca => ca.ClienteId == usuario.Id && ca.ArtistaId == artistaId);

                if (yaAsociado)
                    throw new InvalidOperationException("Este cliente ya está asociado a tu cuenta");
            }
            else
            {
                esNuevoCliente = true;
                // Contraseña genérica: "Cliente123!"
                var passwordGenerica = "Cliente123!";

                usuario = new Usuario
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
            }

            // Asociar cliente al artista
            var clienteArtista = new ClienteArtista
            {
                ClienteId = usuario.Id,
                ArtistaId = artistaId,
                EstudioId = GetEstudioIdActual(),
                Notas = dto.Notas,
                FechaAsociacion = DateTime.UtcNow
            };
            _context.ClientesArtistas.Add(clienteArtista);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(usuario.Id))!;
        }

        private int GetEstudioIdActual()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return 0;

            var estudioId = user.FindFirst("EstudioId")?.Value;
            return estudioId != null ? int.Parse(estudioId) : 0;
        }

        public async Task<ClienteDto?> UpdateAsync(int id, ActualizarClienteDto dto)
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return null;

            var clienteArtista = await _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                .FirstOrDefaultAsync(ca => ca.ClienteId == id && ca.ArtistaId == artistaId);

            if (clienteArtista == null) return null;

            var usuario = clienteArtista.Cliente;

            if (!string.IsNullOrEmpty(dto.Nombre))
                usuario.Nombre = dto.Nombre;

            if (dto.Email != null)
                usuario.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Telefono))
            {
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
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return false;

            var clienteArtista = await _context.ClientesArtistas
                .FirstOrDefaultAsync(ca => ca.ClienteId == id && ca.ArtistaId == artistaId);

            if (clienteArtista == null) return false;

            // Verificar si tiene citas con este artista
            var tieneCitas = await _context.Citas
                .AnyAsync(c => c.UsuarioId == id && c.ArtistaReferenciaId == artistaId && !c.EliminadoLogico);

            if (tieneCitas)
                throw new InvalidOperationException("No se puede eliminar un cliente con citas registradas");

            _context.ClientesArtistas.Remove(clienteArtista);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<ClienteDto>> GetAllAsync(int pagina = 1, int pageSize = 10, string? search = null)
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0)
            {
                return new PagedResult<ClienteDto>
                {
                    Items = new List<ClienteDto>(),
                    TotalCount = 0,
                    PageNumber = pagina,
                    PageSize = pageSize
                };
            }

            var query = _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.UsuarioRoles)
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.CitasComoCliente)
                .Where(ca => ca.ArtistaId == artistaId)
                .Select(ca => ca.Cliente);

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
                Items = items.Select(u => MapToDto(u, null)).ToList(),
                TotalCount = totalCount,
                PageNumber = pagina,
                PageSize = pageSize
            };
        }

        public async Task<int> GetTotalClientesAsync()
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return 0;

            return await _context.ClientesArtistas
                .CountAsync(ca => ca.ArtistaId == artistaId);
        }

        public async Task<List<ClienteDto>> GetClientesFrecuentesAsync(int top = 10)
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return new List<ClienteDto>();

            var clientes = await _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.CitasComoCliente)
                .Where(ca => ca.ArtistaId == artistaId)
                .Select(ca => ca.Cliente)
                .OrderByDescending(c => c.CitasComoCliente.Count)
                .Take(top)
                .ToListAsync();

            return clientes.Select(c => MapToDto(c, null)).ToList();
        }

        public async Task<List<ClienteDto>> GetClientesNuevosPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var artistaId = GetArtistaIdActual();
            if (artistaId == 0) return new List<ClienteDto>();

            var clientes = await _context.ClientesArtistas
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.UsuarioRoles)
                .Include(ca => ca.Cliente)
                    .ThenInclude(c => c.CitasComoCliente)
                .Where(ca => ca.ArtistaId == artistaId
                    && ca.FechaAsociacion >= fechaInicio
                    && ca.FechaAsociacion <= fechaFin)
                .OrderByDescending(ca => ca.FechaAsociacion)
                .Select(ca => ca.Cliente)
                .ToListAsync();

            return clientes.Select(c => MapToDto(c, null)).ToList();
        }

        private ClienteDto MapToDto(Usuario usuario, ClienteArtista? clienteArtista = null)
        {
            return new ClienteDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                FechaRegistro = clienteArtista?.FechaAsociacion ?? usuario.FechaCreacion,
                TotalCitas = usuario.CitasComoCliente?.Count ?? 0,
                TotalGastado = usuario.CitasComoCliente?.Where(c => c.Estado == "completada").Sum(c => c.PrecioTotal) ?? 0
            };
        }
    }
}