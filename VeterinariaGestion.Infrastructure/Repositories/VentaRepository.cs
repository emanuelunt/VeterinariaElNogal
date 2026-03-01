using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Core.Interfaces;
using VeterinariaGestion.Infrastructure.Data;

namespace VeterinariaGestion.Infrastructure.Repositories;

public interface IVentaRepository : IRepository<Venta>
{
    Task<Venta?> GetConDetallesAsync(int id);
    Task<IEnumerable<Venta>> GetByClienteAsync(int idCliente);
    Task<string> GenerarNumeroVentaAsync();
}

public class VentaRepository : BaseRepository<Venta>, IVentaRepository
{
    public VentaRepository(VeterinariaDbContext context) : base(context) { }

    public override async Task<IEnumerable<Venta>> GetAllAsync()
        => await _context.Ventas
                         .Include(v => v.Cliente)
                         .OrderByDescending(v => v.Fecha)
                         .AsNoTracking()
                         .ToListAsync();

    public async Task<Venta?> GetConDetallesAsync(int id)
        => await _context.Ventas
                         .Include(v => v.Cliente)
                         .Include(v => v.Detalles)
                             .ThenInclude(d => d.Producto)
                         .Include(v => v.Cuotas)
                         .AsNoTracking()
                         .FirstOrDefaultAsync(v => v.IdVenta == id);

    public async Task<IEnumerable<Venta>> GetByClienteAsync(int idCliente)
        => await _context.Ventas
                         .Include(v => v.Detalles)
                         .Where(v => v.IdCliente == idCliente)
                         .OrderByDescending(v => v.Fecha)
                         .AsNoTracking()
                         .ToListAsync();

   
   public async Task<string> GenerarNumeroVentaAsync()
    {
        // Busca el número más alto registrado
        var ultima = await _context.Ventas
            .Where(v => v.NumeroVenta != null)
            .OrderByDescending(v => v.IdVenta)
            .FirstOrDefaultAsync();

        int siguiente = 1;

        if (ultima?.NumeroVenta != null)
        {
            // Extrae solo los dígitos del último número
            var soloNumeros = new string(ultima.NumeroVenta
                                            .Where(char.IsDigit)
                                            .ToArray());
            if (int.TryParse(soloNumeros, out int ultimo))
                siguiente = ultimo + 1;
        }

        return siguiente.ToString("D5"); // 00001, 00002, 00003...
    }
   
   
   /**  Antiguo metodo que generaba los numeros de venta con formato V-202406-00001 **/
  /*  public async Task<string> GenerarNumeroVentaAsync()
    {
        var ultimo = await _context.Ventas
                                   .OrderByDescending(v => v.IdVenta)
                                   .FirstOrDefaultAsync();
        int siguiente = (ultimo?.IdVenta ?? 0) + 1;
        return $"V-{DateTime.Now:yyyyMM}-{siguiente:D5}";
    } */

    /*
    Opción 2 — Con prefijo por año (Ej: 2025-00001, 2025-00002...)

    public async Task<string> GenerarNumeroVentaAsync()
        {
            int anioActual = DateTime.Now.Year;

            // Busca la última venta del año actual
            var prefijo = $"{anioActual}-";

            var ultima = await _context.Ventas
                .Where(v => v.NumeroVenta != null &&
                            v.NumeroVenta.StartsWith(prefijo))
                .OrderByDescending(v => v.IdVenta)
                .FirstOrDefaultAsync();

            int siguiente = 1;

            if (ultima?.NumeroVenta != null)
            {
                // Extrae el número después del prefijo "2025-"
                var parteNumero = ultima.NumeroVenta.Replace(prefijo, "");
                if (int.TryParse(parteNumero, out int ultimo))
                    siguiente = ultimo + 1;
            }

            return $"{anioActual}-{siguiente:D5}"; // 2025-00001, 2025-00002...
        }

        Opción 3 — Con prefijo por año y mes (Ej: 202501-00001...)

        public async Task<string> GenerarNumeroVentaAsync()
        {
            var prefijo = DateTime.Now.ToString("yyyyMM") + "-";

            var ultima = await _context.Ventas
                .Where(v => v.NumeroVenta != null &&
                            v.NumeroVenta.StartsWith(prefijo))
                .OrderByDescending(v => v.IdVenta)
                .FirstOrDefaultAsync();

            int siguiente = 1;

            if (ultima?.NumeroVenta != null)
            {
                var parteNumero = ultima.NumeroVenta.Replace(prefijo, "");
                if (int.TryParse(parteNumero, out int ultimo))
                    siguiente = ultimo + 1;
            }

            return $"{prefijo}{siguiente:D5}"; // 202501-00001, 202501-00002...
        }

    */
}