using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Infrastructure.Services;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class ProductosController : Controller
{
    private readonly IProductoService _service;
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10; // productos por página

    public ProductosController(IProductoService service, VeterinariaDbContext context)
    {
        _service = service;
        _context = context;
    }

    /* public async Task<IActionResult> Index(string? buscar, int pagina = 1)
    {
        var todos = await _service.ObtenerTodosAsync();

        // Filtrar por búsqueda
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            todos  = todos.Where(p =>
                p.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                (p.Codigo != null &&
                 p.Codigo.Contains(buscar, StringComparison.OrdinalIgnoreCase)));
        }

        ViewBag.Buscar = buscar;

        var paginado = PaginatedList<Producto>.Create(todos, pagina, PageSize);
        return View(paginado);
    } */

    public async Task<IActionResult> Index(string? buscar, int pagina = 1) // Versión optimizada para paginación en base de datos
    {
        var query = _context.Productos
                            .Include(p => p.Tipo)
                            .Include(p => p.Proveedor)
                            .AsNoTracking()
                            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(p =>
                p.Nombre.Contains(buscar) ||
                (p.Codigo != null && p.Codigo.Contains(buscar)));
        }

        query = query.OrderBy(p => p.Nombre);

        ViewBag.Buscar = buscar;
        return View(await PaginatedList<Producto>.CreateFromQueryAsync(
            query, pagina, PageSize));
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var producto = await _service.ObtenerPorIdAsync(id);
        if (producto is null) return NotFound();
        return View(producto);
    }

    public async Task<IActionResult> Crear()
    {
        await CargarSelectsAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Producto producto)
    {
        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync();
            return View(producto);
        }

        var (exito, mensaje) = await _service.CrearAsync(producto);
        TempData[exito ? "Exito" : "Error"] = mensaje;
        return exito ? RedirectToAction(nameof(Index)) : View(producto);
    }

    public async Task<IActionResult> Editar(int id)
    {
        var producto = await _service.ObtenerPorIdAsync(id);
        if (producto is null) return NotFound();
        await CargarSelectsAsync();
        return View(producto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Producto producto)
    {
        if (id != producto.IdProducto) return BadRequest();

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync();
            return View(producto);
        }

        var (exito, mensaje) = await _service.ActualizarAsync(producto);
        TempData[exito ? "Exito" : "Error"] = mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var (exito, mensaje) = await _service.EliminarAsync(id);
        TempData[exito ? "Exito" : "Error"] = mensaje;
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private async Task CargarSelectsAsync()
    {
        var tipos = await _context.Tipos
            .Where(t => t.Estado == 1)
            .OrderBy(t => t.Descripcion)
            .ToListAsync();

        var proveedores = await _context.Proveedores
            .Where(p => p.Estado == 1)
            .OrderBy(p => p.RazonSocial)
            .ToListAsync();

        ViewBag.Tipos = new SelectList(tipos, "IdTipo", "Descripcion");
        ViewBag.Proveedores = new SelectList(proveedores, "IdProveedor", "RazonSocial");
    }
}
