using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Core.Interfaces;
using VeterinariaGestion.Infrastructure.Data;

namespace VeterinariaGestion.Infrastructure.Repositories;

public interface IMascotaRepository : IRepository<Mascota>
{
    Task<IEnumerable<Mascota>> GetByClienteAsync(int idCliente);
    Task<Mascota?> GetConHistorialAsync(int id);
}

public class MascotaRepository : BaseRepository<Mascota>, IMascotaRepository
{
    public MascotaRepository(VeterinariaDbContext context) : base(context) { }

    public override async Task<IEnumerable<Mascota>> GetAllAsync()
        => await _context.Mascotas
                         .Include(m => m.Cliente)
                         .Include(m => m.Especie)
                         .Where(m => m.Estado == 1)
                         .AsNoTracking()
                         .ToListAsync();

    public async Task<IEnumerable<Mascota>> GetByClienteAsync(int idCliente)
        => await _context.Mascotas
                         .Include(m => m.Especie)
                         .Where(m => m.IdCliente == idCliente && m.Estado == 1)
                         .AsNoTracking()
                         .ToListAsync();

    public async Task<Mascota?> GetConHistorialAsync(int id)
        => await _context.Mascotas
                         .Include(m => m.Cliente)
                         .Include(m => m.Especie)
                         .Include(m => m.Historiales)
                         .Include(m => m.Turnos)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(m => m.IdMascota == id);
}