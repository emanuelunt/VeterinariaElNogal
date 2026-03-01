using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class ProveedoresController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public ProveedoresController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Proveedores
    /*  public async Task<IActionResult> Index(string? buscar, int pagina = 1)
     {
         var query = _context.Proveedores.AsQueryable();

         if (!string.IsNullOrWhiteSpace(buscar))
         {
             buscar = buscar.Trim();
             query  = query.Where(p =>
                 p.RazonSocial.Contains(buscar) ||
                 (p.CuilCuit != null && p.CuilCuit.Contains(buscar)) ||
                 (p.Email    != null && p.Email.Contains(buscar)));
         }

         var lista   = await query.OrderBy(p => p.RazonSocial).ToListAsync();
         ViewBag.Buscar = buscar;

         return View(PaginatedList<Proveedor>.Create(lista, pagina, PageSize));
     } */

    public async Task<IActionResult> Index(string? buscar, int pagina = 1) // Versión optimizada para paginación en base de datos
    {
        var query = _context.Proveedores
                            .AsNoTracking()
                            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(p =>
                p.RazonSocial.Contains(buscar) ||
                (p.CuilCuit != null && p.CuilCuit.Contains(buscar)) ||
                (p.Email != null && p.Email.Contains(buscar)));
        }

        query = query.OrderBy(p => p.RazonSocial);

        ViewBag.Buscar = buscar;
        return View(await PaginatedList<Proveedor>.CreateFromQueryAsync(
            query, pagina, PageSize));
    }

    // GET: /Proveedores/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var proveedor = await _context.Proveedores
                                      .Include(p => p.Productos)
                                      .FirstOrDefaultAsync(p => p.IdProveedor == id);
        if (proveedor is null) return NotFound();
        return View(proveedor);
    }

    // GET: /Proveedores/Crear
    public IActionResult Crear() => View();

    // POST: /Proveedores/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Proveedor proveedor)
    {
        // Verificar CUIL/CUIT duplicado
        if (!string.IsNullOrWhiteSpace(proveedor.CuilCuit))
        {
            bool existe = await _context.Proveedores
                .AnyAsync(p => p.CuilCuit == proveedor.CuilCuit);
            if (existe)
                ModelState.AddModelError("CuilCuit",
                    "Ya existe un proveedor con ese CUIL/CUIT.");
        }

        if (!ModelState.IsValid) return View(proveedor);

        proveedor.FechaAlta = DateTime.Now;
        proveedor.Estado = 1;

        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Proveedor registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Proveedores/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor is null) return NotFound();
        return View(proveedor);
    }

    // POST: /Proveedores/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Proveedor proveedor)
    {
        if (id != proveedor.IdProveedor) return BadRequest();

        // Verificar CUIL/CUIT duplicado excluyendo el actual
        if (!string.IsNullOrWhiteSpace(proveedor.CuilCuit))
        {
            bool existe = await _context.Proveedores
                .AnyAsync(p => p.CuilCuit == proveedor.CuilCuit &&
                               p.IdProveedor != id);
            if (existe)
                ModelState.AddModelError("CuilCuit",
                    "Ya existe otro proveedor con ese CUIL/CUIT.");
        }

        if (!ModelState.IsValid) return View(proveedor);

        var entry = _context.ChangeTracker.Entries<Proveedor>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;

        _context.Entry(proveedor).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Proveedor actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Proveedores/CambiarEstado/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor is null) return NotFound();

        // Toggle: si está activo lo desactiva y viceversa
        proveedor.Estado = proveedor.Estado == 1 ? 0 : 1;
        await _context.SaveChangesAsync();

        string estado = proveedor.Estado == 1 ? "activado" : "desactivado";
        TempData["Exito"] = $"Proveedor {estado} correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Proveedores/Eliminar/5
    public async Task<IActionResult> Eliminar(int id)
    {
        var proveedor = await _context.Proveedores
                                      .Include(p => p.Productos)
                                      .FirstOrDefaultAsync(p => p.IdProveedor == id);
        if (proveedor is null) return NotFound();
        return View(proveedor);
    }

    // POST: /Proveedores/Eliminar/5
    [HttpPost, ActionName("Eliminar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var proveedor = await _context.Proveedores
                                      .Include(p => p.Productos)
                                      .FirstOrDefaultAsync(p => p.IdProveedor == id);
        if (proveedor is null) return NotFound();

        // Si tiene productos asociados, solo desactivar
        if (proveedor.Productos.Any())
        {
            proveedor.Estado = 0;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Proveedor desactivado (tiene productos asociados).";
        }
        else
        {
            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Proveedor eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}