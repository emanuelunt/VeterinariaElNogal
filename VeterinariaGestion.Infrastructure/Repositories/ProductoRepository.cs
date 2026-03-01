using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Core.Interfaces;
using VeterinariaGestion.Infrastructure.Data;

namespace VeterinariaGestion.Infrastructure.Repositories;

public interface IProductoRepository : IRepository<Producto>
{
    Task<IEnumerable<Producto>> GetActivosAsync();
    Task<IEnumerable<Producto>> GetConStockBajoAsync(int minimo = 5);
    Task DescontarStockAsync(int idProducto, int cantidad);
}

public class ProductoRepository : BaseRepository<Producto>, IProductoRepository
{
    public ProductoRepository(VeterinariaDbContext context) : base(context) { }

    public override async Task<IEnumerable<Producto>> GetAllAsync()
        => await _context.Productos
                         .Include(p => p.Tipo)
                         .Include(p => p.Proveedor)
                         .AsNoTracking()
                         .ToListAsync();

    public override async Task<Producto?> GetByIdAsync(int id)
        => await _context.Productos
                         .Include(p => p.Tipo)
                         .Include(p => p.Proveedor)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(p => p.IdProducto == id);

    public async Task<IEnumerable<Producto>> GetActivosAsync()
        => await _context.Productos
                         .Include(p => p.Tipo)
                         .Include(p => p.Proveedor)
                         .Where(p => p.Estado == 1)
                         .AsNoTracking()
                         .ToListAsync();

    public async Task<IEnumerable<Producto>> GetConStockBajoAsync(int minimo = 5)
        => await _context.Productos
                         .Where(p => p.Estado == 1 && p.Stock <= minimo)
                         .AsNoTracking()
                         .ToListAsync();

    public async Task DescontarStockAsync(int idProducto, int cantidad)
    {
        var producto = await _context.Productos.FindAsync(idProducto);
        if (producto is not null)
        {
            producto.Stock -= cantidad;
            await _context.SaveChangesAsync();
        }
    }
}