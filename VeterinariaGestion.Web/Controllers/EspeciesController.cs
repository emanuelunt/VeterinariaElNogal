using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class EspeciesController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public EspeciesController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Especies
    public async Task<IActionResult> Index(string? buscar, int pagina = 1)
    {
        var query = _context.Especies
                            .Include(e => e.Mascotas)
                            .AsNoTracking()
                            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(e => e.Descripcion != null &&
                                     e.Descripcion.Contains(buscar));
        }

        query = query.OrderBy(e => e.Descripcion);

        ViewBag.Buscar = buscar;
        return View(await PaginatedList<Especie>.CreateFromQueryAsync(
            query, pagina, PageSize));
    }

    // GET: /Especies/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var especie = await _context.Especies
                                    .Include(e => e.Mascotas.Where(m => m.Estado == 1))
                                        .ThenInclude(m => m.Cliente)
                                    .FirstOrDefaultAsync(e => e.IdEspecie == id);
        if (especie is null) return NotFound();
        return View(especie);
    }

    // GET: /Especies/Crear
    public IActionResult Crear() => View();

    // POST: /Especies/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Especie especie)
    {
        bool existe = await _context.Especies
            .AnyAsync(e => e.Descripcion != null &&
                           especie.Descripcion != null &&
                           e.Descripcion.ToLower() == especie.Descripcion.ToLower());
        if (existe)
        {
            ModelState.AddModelError("Descripcion",
                "Ya existe una especie con esa descripcion.");
        }

        if (!ModelState.IsValid) return View(especie);

        especie.Estado = 1;
        _context.Especies.Add(especie);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Especie registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Especies/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var especie = await _context.Especies.FindAsync(id);
        if (especie is null) return NotFound();
        return View(especie);
    }

    // POST: /Especies/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Especie especie)
    {
        if (id != especie.IdEspecie) return BadRequest();

        bool existe = await _context.Especies
            .AnyAsync(e => e.Descripcion != null &&
                           especie.Descripcion != null &&
                           e.Descripcion.ToLower() == especie.Descripcion.ToLower() &&
                           e.IdEspecie != id);
        if (existe)
        {
            ModelState.AddModelError("Descripcion",
                "Ya existe otra especie con esa descripcion.");
        }

        if (!ModelState.IsValid) return View(especie);

        var entry = _context.ChangeTracker.Entries<Especie>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;

        _context.Entry(especie).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Especie actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Especies/CambiarEstado/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var especie = await _context.Especies.FindAsync(id);
        if (especie is null) return NotFound();

        especie.Estado = especie.Estado == 1 ? 0 : 1;
        await _context.SaveChangesAsync();

        string estado = especie.Estado == 1 ? "activada" : "desactivada";
        TempData["Exito"] = $"Especie {estado} correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Especies/Eliminar/5
    public async Task<IActionResult> Eliminar(int id)
    {
        var especie = await _context.Especies
                                    .Include(e => e.Mascotas)
                                    .FirstOrDefaultAsync(e => e.IdEspecie == id);
        if (especie is null) return NotFound();
        return View(especie);
    }

    // POST: /Especies/Eliminar/5
    [HttpPost, ActionName("Eliminar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var especie = await _context.Especies
                                    .Include(e => e.Mascotas)
                                    .FirstOrDefaultAsync(e => e.IdEspecie == id);
        if (especie is null) return NotFound();

        if (especie.Mascotas.Any())
        {
            especie.Estado = 0;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Especie desactivada (tiene mascotas asociadas).";
        }
        else
        {
            _context.Especies.Remove(especie);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Especie eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
