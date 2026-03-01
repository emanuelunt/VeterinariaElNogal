using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class CuotasController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public CuotasController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Cuotas
    public async Task<IActionResult> Index(
        string? buscar,
        int? idVenta,
        string? estadoCuota,
        int pagina = 1)
    {
        var query = _context.Cuotas
            .Include(c => c.Venta)
                .ThenInclude(v => v.Cliente)
            .Where(c => c.Estado == 1)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(c =>
                (c.Venta.NumeroVenta != null && c.Venta.NumeroVenta.Contains(buscar)) ||
                (c.Venta.Cliente != null && c.Venta.Cliente.Nombre != null && c.Venta.Cliente.Nombre.Contains(buscar)) ||
                (c.Venta.Cliente != null && c.Venta.Cliente.Apellido != null && c.Venta.Cliente.Apellido.Contains(buscar)) ||
                (c.EstadoCuota != null && c.EstadoCuota.Contains(buscar)));
        }

        if (idVenta.HasValue && idVenta.Value > 0)
            query = query.Where(c => c.IdVenta == idVenta.Value);

        if (!string.IsNullOrWhiteSpace(estadoCuota))
            query = query.Where(c => c.EstadoCuota == estadoCuota);

        query = query.OrderByDescending(c => c.FechaVencimiento)
                     .ThenBy(c => c.NumeroCuota);

        ViewBag.Buscar = buscar;
        ViewBag.IdVenta = idVenta;
        ViewBag.EstadoCuota = estadoCuota;
        await CargarFiltrosAsync(idVenta);

        return View(await PaginatedList<Cuota>.CreateFromQueryAsync(query, pagina, PageSize));
    }

    // GET: /Cuotas/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var cuota = await _context.Cuotas
            .Include(c => c.Venta)
                .ThenInclude(v => v.Cliente)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCuota == id);

        if (cuota is null) return NotFound();
        return View(cuota);
    }

    // GET: /Cuotas/Crear
    public async Task<IActionResult> Crear(int? idVenta = null)
    {
        await CargarSelectsAsync(idVenta);
        return View(new Cuota
        {
            IdVenta = idVenta ?? 0,
            EstadoCuota = EstadoCuotaItem.Pendiente,
            FechaVencimiento = DateTime.Today.AddMonths(1)
        });
    }

    // POST: /Cuotas/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cuota cuota)
    {
        LimpiarModelStateNavegacion();
        await ValidarCuotaAsync(cuota, isEdit: false);
        RecalcularMontosYEstado(cuota);

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(cuota.IdVenta);
            return View(cuota);
        }

        cuota.Estado = 1;
        _context.Cuotas.Add(cuota);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Cuota registrada correctamente.";
        return RedirectToAction(nameof(Index), new { idVenta = cuota.IdVenta });
    }

    // GET: /Cuotas/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var cuota = await _context.Cuotas.FindAsync(id);
        if (cuota is null) return NotFound();

        await CargarSelectsAsync(cuota.IdVenta);
        return View(cuota);
    }

    // POST: /Cuotas/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Cuota cuota)
    {
        if (id != cuota.IdCuota) return BadRequest();

        LimpiarModelStateNavegacion();
        await ValidarCuotaAsync(cuota, isEdit: true);
        RecalcularMontosYEstado(cuota);

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(cuota.IdVenta);
            return View(cuota);
        }

        var entry = _context.ChangeTracker.Entries<Cuota>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;

        _context.Entry(cuota).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Cuota actualizada correctamente.";
        return RedirectToAction(nameof(Index), new { idVenta = cuota.IdVenta });
    }

    // POST: /Cuotas/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cuota = await _context.Cuotas.FindAsync(id);
        if (cuota is null) return NotFound();

        cuota.Estado = 0;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Cuota eliminada correctamente.";
        return RedirectToAction(nameof(Index), new { idVenta = cuota.IdVenta });
    }

    private async Task ValidarCuotaAsync(Cuota cuota, bool isEdit)
    {
        if (cuota.IdVenta <= 0)
            ModelState.AddModelError("IdVenta", "Debe seleccionar una venta.");

        bool ventaValida = await _context.Ventas
            .AnyAsync(v => v.IdVenta == cuota.IdVenta && v.Estado == 1);
        if (!ventaValida)
            ModelState.AddModelError("IdVenta", "La venta seleccionada no es valida.");

        if (cuota.NumeroCuota <= 0)
            ModelState.AddModelError("NumeroCuota", "El numero de cuota debe ser mayor a 0.");

        var duplicada = _context.Cuotas.Where(c =>
            c.IdVenta == cuota.IdVenta &&
            c.NumeroCuota == cuota.NumeroCuota &&
            c.Estado == 1);

        if (isEdit)
            duplicada = duplicada.Where(c => c.IdCuota != cuota.IdCuota);

        if (await duplicada.AnyAsync())
            ModelState.AddModelError("NumeroCuota", "Ya existe una cuota con ese numero para la venta seleccionada.");

        if (cuota.MontoCuota <= 0)
            ModelState.AddModelError("MontoCuota", "El monto de la cuota debe ser mayor a 0.");

        if (cuota.InteresMora < 0)
            ModelState.AddModelError("InteresMora", "El interes de mora no puede ser negativo.");

        if (cuota.MontoPagado < 0)
            ModelState.AddModelError("MontoPagado", "El monto pagado no puede ser negativo.");

        var totalCuota = cuota.MontoCuota + cuota.InteresMora;
        if (cuota.MontoPagado > totalCuota)
            ModelState.AddModelError("MontoPagado", "El monto pagado no puede superar el total de la cuota.");
    }

    private static void RecalcularMontosYEstado(Cuota cuota)
    {
        var totalCuota = cuota.MontoCuota + cuota.InteresMora;
        cuota.SaldoPendiente = Math.Max(0, totalCuota - cuota.MontoPagado);

        if (cuota.MontoPagado <= 0)
        {
            cuota.EstadoCuota = (cuota.FechaVencimiento.HasValue && cuota.FechaVencimiento.Value.Date < DateTime.Today)
                ? EstadoCuotaItem.Vencida
                : EstadoCuotaItem.Pendiente;
            cuota.FechaPago = null;
            return;
        }

        if (cuota.MontoPagado >= totalCuota)
        {
            cuota.EstadoCuota = EstadoCuotaItem.Pagada;
            cuota.FechaPago ??= DateTime.Now;
            return;
        }

        cuota.EstadoCuota = EstadoCuotaItem.Parcial;
        cuota.FechaPago ??= DateTime.Now;
    }

    private async Task CargarSelectsAsync(int? idVentaSeleccionada = null)
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .Where(v => v.Estado == 1)
            .OrderByDescending(v => v.Fecha)
            .AsNoTracking()
            .Select(v => new
            {
                v.IdVenta,
                Texto = (v.NumeroVenta ?? ("Venta #" + v.IdVenta)) + " - " +
                        (v.Cliente != null ? v.Cliente.Apellido + ", " + v.Cliente.Nombre : "Consumidor Final")
            })
            .ToListAsync();

        ViewBag.Ventas = new SelectList(ventas, "IdVenta", "Texto", idVentaSeleccionada);
        ViewBag.EstadosCuota = new List<string>
        {
            EstadoCuotaItem.Pendiente,
            EstadoCuotaItem.Parcial,
            EstadoCuotaItem.Pagada,
            EstadoCuotaItem.Vencida
        };
    }

    private async Task CargarFiltrosAsync(int? idVentaSeleccionada)
    {
        var ventas = await _context.Ventas
            .Where(v => v.Estado == 1)
            .OrderByDescending(v => v.Fecha)
            .AsNoTracking()
            .Select(v => new
            {
                v.IdVenta,
                Texto = v.NumeroVenta ?? ("Venta #" + v.IdVenta)
            })
            .ToListAsync();

        ViewBag.VentasFiltro = new SelectList(ventas, "IdVenta", "Texto", idVentaSeleccionada);
        ViewBag.EstadosCuota = new List<string>
        {
            EstadoCuotaItem.Pendiente,
            EstadoCuotaItem.Parcial,
            EstadoCuotaItem.Pagada,
            EstadoCuotaItem.Vencida
        };
    }

    private void LimpiarModelStateNavegacion()
    {
        ModelState.Remove(nameof(Cuota.Venta));
    }
}

public static class EstadoCuotaItem
{
    public const string Pendiente = "Pendiente";
    public const string Parcial = "Parcial";
    public const string Pagada = "Pagada";
    public const string Vencida = "Vencida";
}
