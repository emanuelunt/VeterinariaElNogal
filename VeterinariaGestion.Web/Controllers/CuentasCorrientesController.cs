using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class CuentasCorrientesController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public CuentasCorrientesController(VeterinariaDbContext context)
        => _context = context;

    // GET: /CuentasCorrientes
    public async Task<IActionResult> Index(
        string? buscar,
        int? idCliente,
        string? estadoCuenta,
        string? tipoMovimiento,
        int pagina = 1)
    {
        var query = _context.CuentasCorrientes
            .Include(c => c.Cliente)
            .Include(c => c.Venta)
            .Where(c => c.Estado == 1)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(c =>
                (c.Cliente.Nombre != null && c.Cliente.Nombre.Contains(buscar)) ||
                (c.Cliente.Apellido != null && c.Cliente.Apellido.Contains(buscar)) ||
                (c.Concepto != null && c.Concepto.Contains(buscar)) ||
                (c.Comprobante != null && c.Comprobante.Contains(buscar)) ||
                (c.Venta != null && c.Venta.NumeroVenta != null && c.Venta.NumeroVenta.Contains(buscar)));
        }

        if (idCliente.HasValue && idCliente.Value > 0)
            query = query.Where(c => c.IdCliente == idCliente.Value);

        if (!string.IsNullOrWhiteSpace(estadoCuenta))
            query = query.Where(c => c.EstadoCuenta == estadoCuenta);

        if (!string.IsNullOrWhiteSpace(tipoMovimiento))
            query = query.Where(c => c.TipoMovimiento == tipoMovimiento);

        query = query.OrderByDescending(c => c.FechaMovimiento)
                     .ThenByDescending(c => c.IdCuentaCorriente);

        ViewBag.Buscar = buscar;
        ViewBag.IdCliente = idCliente;
        ViewBag.EstadoCuenta = estadoCuenta;
        ViewBag.TipoMovimiento = tipoMovimiento;
        await CargarFiltrosAsync(idCliente);

        return View(await PaginatedList<CuentaCorriente>.CreateFromQueryAsync(query, pagina, PageSize));
    }

    // GET: /CuentasCorrientes/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var cuenta = await _context.CuentasCorrientes
            .Include(c => c.Cliente)
            .Include(c => c.Venta)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCuentaCorriente == id);

        if (cuenta is null) return NotFound();
        return View(cuenta);
    }

    // GET: /CuentasCorrientes/Crear
    public async Task<IActionResult> Crear(int? idCliente = null)
    {
        await CargarSelectsAsync(idCliente, null);
        return View(new CuentaCorriente
        {
            IdCliente = idCliente ?? 0,
            FechaMovimiento = DateTime.Now,
            TipoMovimiento = TipoMovimientoCuenta.Debito,
            EstadoCuenta = EstadoCuentaCorriente.Pendiente
        });
    }

    // POST: /CuentasCorrientes/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CuentaCorriente cuenta)
    {
        LimpiarModelStateNavegacion();
        await ValidarCuentaCorrienteAsync(cuenta);

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(cuenta.IdCliente, cuenta.IdVenta);
            return View(cuenta);
        }

        cuenta.FechaMovimiento ??= DateTime.Now;
        cuenta.EstadoCuenta ??= EstadoCuentaCorriente.Pendiente;
        cuenta.Estado = 1;

        cuenta.SaldoAnterior = await ObtenerUltimoSaldoClienteAsync(cuenta.IdCliente);
        cuenta.SaldoNuevo = CalcularSaldoNuevo(
            cuenta.SaldoAnterior,
            cuenta.Importe,
            cuenta.TipoMovimiento);

        _context.CuentasCorrientes.Add(cuenta);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Movimiento de cuenta corriente registrado correctamente.";
        return RedirectToAction(nameof(Index), new { idCliente = cuenta.IdCliente });
    }

    // GET: /CuentasCorrientes/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var cuenta = await _context.CuentasCorrientes.FindAsync(id);
        if (cuenta is null) return NotFound();

        await CargarSelectsAsync(cuenta.IdCliente, cuenta.IdVenta, cuenta.IdCuentaCorriente);
        return View(cuenta);
    }

    // POST: /CuentasCorrientes/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, CuentaCorriente cuenta)
    {
        if (id != cuenta.IdCuentaCorriente) return BadRequest();

        LimpiarModelStateNavegacion();
        await ValidarCuentaCorrienteAsync(cuenta);

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(cuenta.IdCliente, cuenta.IdVenta, cuenta.IdCuentaCorriente);
            return View(cuenta);
        }

        var actual = await _context.CuentasCorrientes.FirstOrDefaultAsync(c => c.IdCuentaCorriente == id);
        if (actual is null) return NotFound();

        // Si cambia cliente, recalcula saldo base sobre ese cliente.
        if (actual.IdCliente != cuenta.IdCliente)
            cuenta.SaldoAnterior = await ObtenerUltimoSaldoClienteAsync(cuenta.IdCliente, cuenta.IdCuentaCorriente);
        else
            cuenta.SaldoAnterior = actual.SaldoAnterior;

        cuenta.SaldoNuevo = CalcularSaldoNuevo(
            cuenta.SaldoAnterior,
            cuenta.Importe,
            cuenta.TipoMovimiento);

        actual.IdCliente = cuenta.IdCliente;
        actual.IdVenta = cuenta.IdVenta;
        actual.FechaMovimiento = cuenta.FechaMovimiento;
        actual.TipoMovimiento = cuenta.TipoMovimiento;
        actual.Concepto = cuenta.Concepto;
        actual.Importe = cuenta.Importe;
        actual.SaldoAnterior = cuenta.SaldoAnterior;
        actual.SaldoNuevo = cuenta.SaldoNuevo;
        actual.FechaVencimiento = cuenta.FechaVencimiento;
        actual.EstadoCuenta = cuenta.EstadoCuenta;
        actual.Comprobante = cuenta.Comprobante;
        actual.Observacion = cuenta.Observacion;
        actual.Estado = cuenta.Estado;

        await _context.SaveChangesAsync();

        TempData["Exito"] = "Movimiento actualizado correctamente.";
        return RedirectToAction(nameof(Index), new { idCliente = cuenta.IdCliente });
    }

    // POST: /CuentasCorrientes/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cuenta = await _context.CuentasCorrientes.FindAsync(id);
        if (cuenta is null) return NotFound();

        cuenta.Estado = 0;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Movimiento eliminado correctamente.";
        return RedirectToAction(nameof(Index), new { idCliente = cuenta.IdCliente });
    }

    private async Task ValidarCuentaCorrienteAsync(CuentaCorriente cuenta)
    {
        if (cuenta.IdCliente <= 0)
            ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente.");

        bool clienteExiste = await _context.Clientes
            .AnyAsync(c => c.IdCliente == cuenta.IdCliente && c.Estado == 1);
        if (!clienteExiste)
            ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente activo.");

        if (cuenta.Importe <= 0)
            ModelState.AddModelError("Importe", "El importe debe ser mayor a 0.");

        if (string.IsNullOrWhiteSpace(cuenta.TipoMovimiento))
            ModelState.AddModelError("TipoMovimiento", "Debe seleccionar un tipo de movimiento.");
        else if (cuenta.TipoMovimiento != TipoMovimientoCuenta.Debito &&
                 cuenta.TipoMovimiento != TipoMovimientoCuenta.Credito)
            ModelState.AddModelError("TipoMovimiento", "Tipo de movimiento invalido.");

        if (cuenta.IdVenta.HasValue && cuenta.IdVenta.Value > 0)
        {
            bool ventaValida = await _context.Ventas
                .AnyAsync(v => v.IdVenta == cuenta.IdVenta.Value &&
                               v.Estado == 1 &&
                               v.IdCliente == cuenta.IdCliente);
            if (!ventaValida)
                ModelState.AddModelError("IdVenta", "La venta seleccionada no pertenece al cliente o no es valida.");
        }
    }

    private async Task<decimal> ObtenerUltimoSaldoClienteAsync(int idCliente, int? excluirId = null)
    {
        var query = _context.CuentasCorrientes
            .Where(c => c.IdCliente == idCliente && c.Estado == 1);

        if (excluirId.HasValue)
            query = query.Where(c => c.IdCuentaCorriente != excluirId.Value);

        var ultimo = await query
            .OrderByDescending(c => c.FechaMovimiento)
            .ThenByDescending(c => c.IdCuentaCorriente)
            .FirstOrDefaultAsync();

        return ultimo?.SaldoNuevo ?? 0m;
    }

    private static decimal CalcularSaldoNuevo(decimal saldoAnterior, decimal importe, string? tipoMovimiento)
    {
        return tipoMovimiento == TipoMovimientoCuenta.Credito
            ? saldoAnterior - importe
            : saldoAnterior + importe;
    }

    private async Task CargarSelectsAsync(int? idClienteSeleccionado, int? idVentaSeleccionada, int? idCuentaActual = null)
    {
        var clientes = await _context.Clientes
            .Where(c => c.Estado == 1)
            .OrderBy(c => c.Apellido)
            .ThenBy(c => c.Nombre)
            .AsNoTracking()
            .ToListAsync();

        var ventasBase = _context.Ventas
            .Where(v => v.Estado == 1 && v.IdCliente.HasValue)
            .AsNoTracking()
            .AsQueryable();

        if (idClienteSeleccionado.HasValue && idClienteSeleccionado.Value > 0)
            ventasBase = ventasBase.Where(v => v.IdCliente == idClienteSeleccionado.Value);

        var ventas = await ventasBase
            .OrderByDescending(v => v.Fecha)
            .Select(v => new
            {
                v.IdVenta,
                Texto = (v.NumeroVenta ?? ("Venta #" + v.IdVenta)) +
                        " - " + (v.Fecha.HasValue ? v.Fecha.Value.ToString("dd/MM/yyyy") : "--/--/----")
            })
            .ToListAsync();

        ViewBag.Clientes = new SelectList(clientes, "IdCliente", "NombreCompleto", idClienteSeleccionado);
        ViewBag.Ventas = new SelectList(ventas, "IdVenta", "Texto", idVentaSeleccionada);
        ViewBag.TiposMovimiento = new List<string>
        {
            TipoMovimientoCuenta.Debito,
            TipoMovimientoCuenta.Credito
        };
        ViewBag.EstadosCuenta = new List<string>
        {
            EstadoCuentaCorriente.Pendiente,
            EstadoCuentaCorriente.Pagada,
            EstadoCuentaCorriente.Vencida,
            EstadoCuentaCorriente.Cancelada
        };
    }

    private async Task CargarFiltrosAsync(int? idClienteSeleccionado)
    {
        var clientes = await _context.Clientes
            .Where(c => c.Estado == 1)
            .OrderBy(c => c.Apellido)
            .ThenBy(c => c.Nombre)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.ClientesFiltro = new SelectList(clientes, "IdCliente", "NombreCompleto", idClienteSeleccionado);
        ViewBag.TiposMovimiento = new List<string>
        {
            TipoMovimientoCuenta.Debito,
            TipoMovimientoCuenta.Credito
        };
        ViewBag.EstadosCuenta = new List<string>
        {
            EstadoCuentaCorriente.Pendiente,
            EstadoCuentaCorriente.Pagada,
            EstadoCuentaCorriente.Vencida,
            EstadoCuentaCorriente.Cancelada
        };
    }

    private void LimpiarModelStateNavegacion()
    {
        ModelState.Remove(nameof(CuentaCorriente.Cliente));
        ModelState.Remove(nameof(CuentaCorriente.Venta));
    }
}

public static class TipoMovimientoCuenta
{
    public const string Debito = "Debito";
    public const string Credito = "Credito";
}

public static class EstadoCuentaCorriente
{
    public const string Pendiente = "Pendiente";
    public const string Pagada = "Pagada";
    public const string Vencida = "Vencida";
    public const string Cancelada = "Cancelada";
}
