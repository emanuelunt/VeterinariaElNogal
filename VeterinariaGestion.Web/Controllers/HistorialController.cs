using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class HistorialController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public HistorialController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Historial
    public async Task<IActionResult> Index(
        string? buscar,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        int pagina = 1)
    {
        var query = _context.Historiales
            .Include(h => h.Mascota)
                .ThenInclude(m => m.Cliente)
            .Include(h => h.Mascota)
                .ThenInclude(m => m.Especie)
            .Include(h => h.Turno)
            .Where(h => h.Estado == 1)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(h =>
                (h.Mascota.Nombre != null && h.Mascota.Nombre.Contains(buscar)) ||
                (h.Mascota.Cliente.Nombre != null && h.Mascota.Cliente.Nombre.Contains(buscar)) ||
                (h.Mascota.Cliente.Apellido != null && h.Mascota.Cliente.Apellido.Contains(buscar)) ||
                (h.MotivoConsulta != null && h.MotivoConsulta.Contains(buscar)) ||
                (h.Diagnostico != null && h.Diagnostico.Contains(buscar)));
        }

        if (fechaDesde.HasValue)
            query = query.Where(h => h.FechaConsulta.HasValue && h.FechaConsulta.Value.Date >= fechaDesde.Value.Date);

        if (fechaHasta.HasValue)
            query = query.Where(h => h.FechaConsulta.HasValue && h.FechaConsulta.Value.Date <= fechaHasta.Value.Date);

        query = query.OrderByDescending(h => h.FechaConsulta)
                     .ThenByDescending(h => h.IdHistorial);

        ViewBag.Buscar = buscar;
        ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
        ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

        return View(await PaginatedList<Historial>.CreateFromQueryAsync(query, pagina, PageSize));
    }

    // GET: /Historial/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var historial = await _context.Historiales
            .Include(h => h.Mascota)
                .ThenInclude(m => m.Cliente)
            .Include(h => h.Mascota)
                .ThenInclude(m => m.Especie)
            .Include(h => h.Turno)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.IdHistorial == id);

        if (historial is null) return NotFound();
        return View(historial);
    }

    // GET: /Historial/Crear
    public async Task<IActionResult> Crear(int? idMascota = null, int? idTurno = null)
    {
        await CargarSelectsAsync(idMascota, idTurno);
        return View(new Historial
        {
            IdMascota = idMascota ?? 0,
            IdTurno = idTurno,
            FechaConsulta = DateTime.Now
        });
    }

    // POST: /Historial/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Historial historial)
    {
        LimpiarModelStateNavegacion();
        await ValidarHistorialAsync(historial, isEdit: false);

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(historial.IdMascota, historial.IdTurno);
            return View(historial);
        }

        historial.Estado = 1;
        historial.FechaConsulta ??= DateTime.Now;

        _context.Historiales.Add(historial);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Historial registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Historial/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var historial = await _context.Historiales.FindAsync(id);
        if (historial is null) return NotFound();

        await CargarSelectsAsync(historial.IdMascota, historial.IdTurno, historial.IdHistorial);
        return View(historial);
    }

    // POST: /Historial/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Historial historial)
    {
        if (id != historial.IdHistorial) return BadRequest();

        LimpiarModelStateNavegacion();
        await ValidarHistorialAsync(historial, isEdit: true);

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(historial.IdMascota, historial.IdTurno, historial.IdHistorial);
            return View(historial);
        }

        var entry = _context.ChangeTracker.Entries<Historial>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;

        _context.Entry(historial).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Historial actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Historial/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var historial = await _context.Historiales.FindAsync(id);
        if (historial is null) return NotFound();

        historial.Estado = 0;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Historial eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidarHistorialAsync(Historial historial, bool isEdit)
    {
        if (historial.IdMascota <= 0)
            ModelState.AddModelError("IdMascota", "Debe seleccionar una mascota.");

        bool mascotaExiste = await _context.Mascotas
            .AnyAsync(m => m.IdMascota == historial.IdMascota && m.Estado == 1);
        if (!mascotaExiste)
            ModelState.AddModelError("IdMascota", "Debe seleccionar una mascota activa.");

        if (!historial.FechaConsulta.HasValue)
            ModelState.AddModelError("FechaConsulta", "La fecha de consulta es obligatoria.");

        if (historial.IdTurno.HasValue && historial.IdTurno.Value > 0)
        {
            bool turnoExiste = await _context.Turnos
                .AnyAsync(t => t.IdTurno == historial.IdTurno.Value &&
                               t.Estado == 1 &&
                               t.EstadoTurno != EstadoTurno.Cancelado);
            if (!turnoExiste)
                ModelState.AddModelError("IdTurno", "El turno seleccionado no es valido.");

            var query = _context.Historiales
                .Where(h => h.IdTurno == historial.IdTurno.Value && h.Estado == 1);

            if (isEdit)
                query = query.Where(h => h.IdHistorial != historial.IdHistorial);

            if (await query.AnyAsync())
                ModelState.AddModelError("IdTurno", "Ese turno ya esta asociado a otro historial.");
        }
    }

    private async Task CargarSelectsAsync(int? idMascotaSeleccionada = null, int? idTurnoSeleccionado = null, int? idHistorialActual = null)
    {
        var mascotas = await _context.Mascotas
            .Include(m => m.Cliente)
            .Include(m => m.Especie)
            .Where(m => m.Estado == 1)
            .OrderBy(m => m.Nombre)
            .AsNoTracking()
            .Select(m => new
            {
                m.IdMascota,
                Texto = (m.Nombre ?? "Sin nombre") + " - " +
                        (m.Cliente.Apellido ?? "") + ", " + (m.Cliente.Nombre ?? "") +
                        " (" + (m.Especie.Descripcion ?? "Sin especie") + ")"
            })
            .ToListAsync();

        var turnosBase = _context.Turnos
            .Include(t => t.Mascota)
            .Where(t => t.Estado == 1 && t.EstadoTurno != EstadoTurno.Cancelado)
            .AsNoTracking()
            .AsQueryable();

        // Si estamos creando/editar y hay mascota elegida, filtramos turnos de esa mascota.
        if (idMascotaSeleccionada.HasValue && idMascotaSeleccionada.Value > 0)
            turnosBase = turnosBase.Where(t => t.IdMascota == idMascotaSeleccionada.Value);

        var turnosConHistorial = _context.Historiales
            .Where(h => h.IdTurno.HasValue && h.Estado == 1)
            .Select(h => h.IdTurno!.Value);

        var turnos = await turnosBase
            .Where(t => !turnosConHistorial.Contains(t.IdTurno) || (idTurnoSeleccionado.HasValue && t.IdTurno == idTurnoSeleccionado.Value))
            .OrderByDescending(t => t.Fecha)
            .ThenBy(t => t.HoraTurno)
            .Select(t => new
            {
                t.IdTurno,
                Texto = (t.Fecha.HasValue ? t.Fecha.Value.ToString("dd/MM/yyyy") : "--/--/----") +
                        " " + (t.HoraTurno ?? "--:--") +
                        " - " + (t.Mascota.Nombre ?? "Sin mascota")
            })
            .ToListAsync();

        ViewBag.Mascotas = new SelectList(mascotas, "IdMascota", "Texto", idMascotaSeleccionada);
        ViewBag.Turnos = new SelectList(turnos, "IdTurno", "Texto", idTurnoSeleccionado);
    }

    private void LimpiarModelStateNavegacion()
    {
        ModelState.Remove(nameof(Historial.Mascota));
        ModelState.Remove(nameof(Historial.Turno));
    }
}
