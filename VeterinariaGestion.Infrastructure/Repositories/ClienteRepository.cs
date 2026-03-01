using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Core.Interfaces;
using VeterinariaGestion.Infrastructure.Data;

namespace VeterinariaGestion.Infrastructure.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<IEnumerable<Cliente>> GetActivosAsync();
    Task<IEnumerable<Cliente>> BuscarAsync(string termino);
    Task<Cliente?> GetConMascotasAsync(int id);
}

public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
{
    public ClienteRepository(VeterinariaDbContext context) : base(context) { }

    public override async Task<IEnumerable<Cliente>> GetAllAsync()
        => await _context.Clientes
                         .Where(c => c.Estado == 1)
                         .OrderBy(c => c.Apellido)
                         .AsNoTracking()
                         .ToListAsync();

    public async Task<IEnumerable<Cliente>> GetActivosAsync()
        => await GetAllAsync();

    public async Task<IEnumerable<Cliente>> BuscarAsync(string termino)
        => await _context.Clientes
                         .Where(c => c.Estado == 1 &&
                                    (c.Nombre!.Contains(termino) ||
                                     c.Apellido!.Contains(termino) ||
                                     c.CuilDni!.Contains(termino)))
                         .AsNoTracking()
                         .ToListAsync();

    public async Task<Cliente?> GetConMascotasAsync(int id)
        => await _context.Clientes
                         .Include(c => c.Mascotas)
                             .ThenInclude(m => m.Especie)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(c => c.IdCliente == id);
}