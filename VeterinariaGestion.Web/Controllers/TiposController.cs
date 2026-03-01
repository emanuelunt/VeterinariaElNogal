using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class TiposController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public TiposController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Tipos
    /* public async Task<IActionResult> Index(string? buscar, int pagina = 1)
    {
        var query = _context.Tipos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query  = query.Where(t => t.Descripcion.Contains(buscar));
        }

        var lista = await query.OrderBy(t => t.Descripcion).ToListAsync();
        ViewBag.Buscar = buscar;

        return View(PaginatedList<Tipo>.Create(lista, pagina, PageSize));
    } */

    public async Task<IActionResult> Index(string? buscar, int pagina = 1) // Versión optimizada para paginación en base de datos
    {
        var query = _context.Tipos
                            .AsNoTracking()
                            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(t => t.Descripcion.Contains(buscar));
        }

        query = query.OrderBy(t => t.Descripcion);

        ViewBag.Buscar = buscar;
        return View(await PaginatedList<Tipo>.CreateFromQueryAsync(
            query, pagina, PageSize));
    }

    // GET: /Tipos/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var tipo = await _context.Tipos
                                 .Include(t => t.Productos.Where(p => p.Estado == 1))
                                 .FirstOrDefaultAsync(t => t.IdTipo == id);
        if (tipo is null) return NotFound();
        return View(tipo);
    }

    // GET: /Tipos/Crear
    public IActionResult Crear() => View();

    // POST: /Tipos/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Tipo tipo)
    {
        // Verificar descripción duplicada
        bool existe = await _context.Tipos
            .AnyAsync(t => t.Descripcion.ToLower() == tipo.Descripcion.ToLower());
        if (existe)
            ModelState.AddModelError("Descripcion",
                "Ya existe un tipo con esa descripción.");

        if (!ModelState.IsValid) return View(tipo);

        tipo.Estado = 1;
        _context.Tipos.Add(tipo);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Tipo registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Tipos/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var tipo = await _context.Tipos.FindAsync(id);
        if (tipo is null) return NotFound();
        return View(tipo);
    }

    // POST: /Tipos/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Tipo tipo)
    {
        if (id != tipo.IdTipo) return BadRequest();

        // Verificar duplicado excluyendo el actual
        bool existe = await _context.Tipos
            .AnyAsync(t => t.Descripcion.ToLower() == tipo.Descripcion.ToLower()
                        && t.IdTipo != id);
        if (existe)
            ModelState.AddModelError("Descripcion",
                "Ya existe otro tipo con esa descripción.");

        if (!ModelState.IsValid) return View(tipo);

        var entry = _context.ChangeTracker.Entries<Tipo>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;

        _context.Entry(tipo).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Tipo actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Tipos/CambiarEstado/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var tipo = await _context.Tipos.FindAsync(id);
        if (tipo is null) return NotFound();

        tipo.Estado = tipo.Estado == 1 ? 0 : 1;
        await _context.SaveChangesAsync();

        string estado = tipo.Estado == 1 ? "activado" : "desactivado";
        TempData["Exito"] = $"Tipo {estado} correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Tipos/Eliminar/5
    public async Task<IActionResult> Eliminar(int id)
    {
        var tipo = await _context.Tipos
                                 .Include(t => t.Productos)
                                 .FirstOrDefaultAsync(t => t.IdTipo == id);
        if (tipo is null) return NotFound();
        return View(tipo);
    }

    // POST: /Tipos/Eliminar/5
    [HttpPost, ActionName("Eliminar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var tipo = await _context.Tipos
                                 .Include(t => t.Productos)
                                 .FirstOrDefaultAsync(t => t.IdTipo == id);
        if (tipo is null) return NotFound();

        // Si tiene productos asociados solo desactivar
        if (tipo.Productos.Any())
        {
            tipo.Estado = 0;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Tipo desactivado (tiene productos asociados).";
        }
        else
        {
            _context.Tipos.Remove(tipo);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Tipo eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}