using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Infrastructure.Services;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class VentasController : Controller
{
    private readonly IVentaService        _ventaService;
    private readonly IClienteService      _clienteService;
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public VentasController(
        IVentaService        ventaService,
        IClienteService      clienteService,
        VeterinariaDbContext context)
    {
        _ventaService   = ventaService;
        _clienteService = clienteService;
        _context        = context;
    }

    // GET: /Ventas
    public async Task<IActionResult> Index(
        string? buscar,
        string? estado,
        int     pagina = 1)
    {
        // ── Construir query SIN ejecutar aún ──────────────────────────
        var query = _context.Ventas
                            .Include(v => v.Cliente)
                            .AsNoTracking()
                            .AsQueryable();

        // ── Filtros (se traducen a SQL WHERE) ─────────────────────────
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query  = query.Where(v =>
                (v.NumeroVenta != null &&
                 v.NumeroVenta.Contains(buscar)) ||
                (v.Cliente != null &&
                    (v.Cliente.Nombre!.Contains(buscar) ||
                     v.Cliente.Apellido!.Contains(buscar))));
        }

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(v => v.EstadoPago == estado);

        // ── Ordenar antes de paginar ──────────────────────────────────
        query = query.OrderByDescending(v => v.Fecha);

        // ── Paginación en BD: ejecuta COUNT + SELECT con LIMIT ────────
        var paginado = await PaginatedList<Venta>.CreateFromQueryAsync(
            query, pagina, PageSize);

        ViewBag.Buscar  = buscar;
        ViewBag.Estado  = estado;
        ViewBag.Estados = new List<string>
        {
            EstadoPago.Pendiente,
            EstadoPago.Pagado,
            EstadoPago.Parcial,
            EstadoPago.Anulado
        };

        return View(paginado);
    }

    // GET: /Ventas/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(v => v.Cuotas)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdVenta == id);

        if (venta is null) return NotFound();
        return View(venta);
    }

    // GET: /Ventas/Crear
    public async Task<IActionResult> Crear()
    {
        await CargarViewBagCrear();
        return View();
    }

    // POST: /Ventas/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        Venta         venta,
        List<int>     productoIds,
        List<int>     cantidades,
        List<decimal> precios,
        List<decimal> descuentos,
        int           cantidadCuotas = 1)
    {
        if (!productoIds.Any())
        {
            TempData["Error"] = "Debe agregar al menos un producto.";
            await CargarViewBagCrear();
            return View(venta);
        }

        var detalles = productoIds.Select((idProd, i) => new VentaDetalle
        {
            IdProducto     = idProd,
            Cantidad       = cantidades[i],
            PrecioUnitario = precios[i],
            DescuentoItem  = descuentos.Count > i ? descuentos[i] : 0
        }).ToList();

        var (exito, mensaje, idVenta) =
            await _ventaService.CrearVentaAsync(venta, detalles, cantidadCuotas);

        if (!exito)
        {
            TempData["Error"] = mensaje;
            await CargarViewBagCrear();
            return View(venta);
        }

        TempData["Exito"] = mensaje;
        return RedirectToAction(nameof(Detalle), new { id = idVenta });
    }

    // POST: /Ventas/Anular/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var (exito, mensaje) = await _ventaService.AnularVentaAsync(id);
        TempData[exito ? "Exito" : "Error"] = mensaje;
        return RedirectToAction(nameof(Detalle), new { id });
    }

    // GET: /Ventas/BuscarProductos?termino=xxx
    [HttpGet]
    public async Task<IActionResult> BuscarProductos(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return Json(new List<object>());

        var productos = await _context.Productos
            .Where(p => p.Estado == 1 && p.Stock > 0 &&
                       (p.Nombre.Contains(termino) ||
                        (p.Codigo != null &&
                         p.Codigo.Contains(termino))))
            .Select(p => new
            {
                id              = p.IdProducto,
                codigo          = p.Codigo ?? "",
                nombre          = p.Nombre,
                stock           = p.Stock,
                precioMinorista = p.PrecioMinorista,
                precioMayorista = p.PrecioMayorista
            })
            .Take(10)
            .AsNoTracking()
            .ToListAsync();

        return Json(productos);
    }

    // ── Helper privado ────────────────────────────────────────────────
    private async Task CargarViewBagCrear()
    {
        ViewBag.Clientes = await _clienteService.ObtenerTodosAsync();
        ViewBag.FormasPago = new List<string>
        {
            FormaPago.Efectivo,
            FormaPago.Tarjeta,
            FormaPago.Transferencia,
            FormaPago.CuentaCorriente
        };
    }
}