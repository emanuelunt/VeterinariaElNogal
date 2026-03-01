using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class MascotasController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public MascotasController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Mascotas
    public async Task<IActionResult> Index(string? buscar, int? idCliente, int pagina = 1)
    {
        var query = _context.Mascotas
                            .Include(m => m.Cliente)
                            .Include(m => m.Especie)
                            .Where(m => m.Estado == 1)
                            .AsNoTracking()
                            .AsQueryable();

        if (idCliente.HasValue)
            query = query.Where(m => m.IdCliente == idCliente.Value);

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(m =>
                (m.Nombre != null && m.Nombre.Contains(buscar)) ||
                (m.Cliente.Nombre != null && m.Cliente.Nombre.Contains(buscar)) ||
                (m.Cliente.Apellido != null && m.Cliente.Apellido.Contains(buscar)) ||
                (m.Especie.Descripcion != null && m.Especie.Descripcion.Contains(buscar)));
        }

        query = query.OrderBy(m => m.Nombre);

        ViewBag.Buscar = buscar;
        ViewBag.IdCliente = idCliente;
        return View(await PaginatedList<Mascota>.CreateFromQueryAsync(query, pagina, PageSize));
    }

    // GET: /Mascotas/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var mascota = await _context.Mascotas
                                    .Include(m => m.Cliente)
                                    .Include(m => m.Especie)
                                    .Include(m => m.Turnos)
                                    .Include(m => m.Historiales)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(m => m.IdMascota == id);
        if (mascota is null) return NotFound();
        return View(mascota);
    }

    // GET: /Mascotas/Crear
    public async Task<IActionResult> Crear(int? idCliente = null)
    {
        await CargarSelectsAsync(idCliente);
        return View(new Mascota { IdCliente = idCliente ?? 0 });
    }

    // POST: /Mascotas/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Mascota mascota)
    {
        ModelState.Remove(nameof(Mascota.Cliente));
        ModelState.Remove(nameof(Mascota.Especie));
        ModelState.Remove(nameof(Mascota.Turnos));
        ModelState.Remove(nameof(Mascota.Historiales));

        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
        if (mascota.IdCliente <= 0)
            ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente.");
        if (mascota.IdEspecie <= 0)
            ModelState.AddModelError("IdEspecie", "Debe seleccionar una especie.");

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(mascota.IdCliente);
            return View(mascota);
        }

        bool clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == mascota.IdCliente && c.Estado == 1);
        if (!clienteExiste)
            ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente activo.");

        bool especieExiste = await _context.Especies.AnyAsync(e => e.IdEspecie == mascota.IdEspecie && e.Estado == 1);
        if (!especieExiste)
            ModelState.AddModelError("IdEspecie", "Debe seleccionar una especie activa.");

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(mascota.IdCliente);
            return View(mascota);
        }

        try
        {
            mascota.Estado = 1;
            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo guardar la mascota. Verifique cliente y especie.");
            await CargarSelectsAsync(mascota.IdCliente);
            return View(mascota);
        }

        TempData["Exito"] = "Mascota registrada correctamente.";
        return RedirectToAction(nameof(Index), new { idCliente = mascota.IdCliente });
    }

    // GET: /Mascotas/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var mascota = await _context.Mascotas.FindAsync(id);
        if (mascota is null) return NotFound();
        await CargarSelectsAsync(mascota.IdCliente);
        return View(mascota);
    }

    // POST: /Mascotas/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Mascota mascota)
    {
        if (id != mascota.IdMascota) return BadRequest();

        ModelState.Remove(nameof(Mascota.Cliente));
        ModelState.Remove(nameof(Mascota.Especie));
        ModelState.Remove(nameof(Mascota.Turnos));
        ModelState.Remove(nameof(Mascota.Historiales));

        if (string.IsNullOrWhiteSpace(mascota.Nombre))
            ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
        if (mascota.IdCliente <= 0)
            ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente.");
        if (mascota.IdEspecie <= 0)
            ModelState.AddModelError("IdEspecie", "Debe seleccionar una especie.");

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(mascota.IdCliente);
            return View(mascota);
        }

        bool clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == mascota.IdCliente && c.Estado == 1);
        if (!clienteExiste)
            ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente activo.");

        bool especieExiste = await _context.Especies.AnyAsync(e => e.IdEspecie == mascota.IdEspecie && e.Estado == 1);
        if (!especieExiste)
            ModelState.AddModelError("IdEspecie", "Debe seleccionar una especie activa.");

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(mascota.IdCliente);
            return View(mascota);
        }

        var entry = _context.ChangeTracker.Entries<Mascota>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;

        try
        {
            _context.Entry(mascota).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "No se pudo actualizar la mascota.");
            await CargarSelectsAsync(mascota.IdCliente);
            return View(mascota);
        }

        TempData["Exito"] = "Mascota actualizada correctamente.";
        return RedirectToAction(nameof(Index), new { idCliente = mascota.IdCliente });
    }

    // POST: /Mascotas/CambiarEstado/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var mascota = await _context.Mascotas.FindAsync(id);
        if (mascota is null) return NotFound();

        mascota.Estado = mascota.Estado == 1 ? 0 : 1;
        await _context.SaveChangesAsync();

        string estado = mascota.Estado == 1 ? "activada" : "desactivada";
        TempData["Exito"] = $"Mascota {estado} correctamente.";
        return RedirectToAction(nameof(Index), new { idCliente = mascota.IdCliente });
    }

    // POST: /Mascotas/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var mascota = await _context.Mascotas
                                    .Include(m => m.Turnos)
                                    .Include(m => m.Historiales)
                                    .FirstOrDefaultAsync(m => m.IdMascota == id);
        if (mascota is null) return NotFound();

        if (mascota.Turnos.Any() || mascota.Historiales.Any())
        {
            mascota.Estado = 0;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Mascota desactivada (tiene turnos o historial asociados).";
        }
        else
        {
            _context.Mascotas.Remove(mascota);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Mascota eliminada correctamente.";
        }

        return RedirectToAction(nameof(Index), new { idCliente = mascota.IdCliente });
    }

    private async Task CargarSelectsAsync(int? idClienteSeleccionado = null)
    {
        var clientes = await _context.Clientes
                                     .Where(c => c.Estado == 1)
                                     .OrderBy(c => c.Apellido)
                                     .ThenBy(c => c.Nombre)
                                     .AsNoTracking()
                                     .ToListAsync();
        var especies = await _context.Especies
                                     .Where(e => e.Estado == 1)
                                     .OrderBy(e => e.Descripcion)
                                     .AsNoTracking()
                                     .ToListAsync();

        ViewBag.Clientes = new SelectList(clientes, "IdCliente", "NombreCompleto", idClienteSeleccionado);
        ViewBag.Especies = new SelectList(especies, "IdEspecie", "Descripcion");
        ViewBag.Sexos = new SelectList(new List<string> { "Macho", "Hembra" });
    }
}
