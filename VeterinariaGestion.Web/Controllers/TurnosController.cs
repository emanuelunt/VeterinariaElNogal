using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class TurnosController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public TurnosController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Turnos
    public async Task<IActionResult> Index(
        string? buscar,
        string? estado,
        DateTime? fecha,
        int pagina = 1)
    {
        var query = _context.Turnos
                            .Include(t => t.Mascota)
                                .ThenInclude(m => m.Cliente)
                            .Include(t => t.Mascota)
                                .ThenInclude(m => m.Especie)
                            .Where(t => t.Estado == 1)
                            .AsNoTracking()
                            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(t =>
                (t.Mascota.Nombre != null && t.Mascota.Nombre.Contains(buscar)) ||
                (t.Mascota.Cliente.Nombre != null && t.Mascota.Cliente.Nombre.Contains(buscar)) ||
                (t.Mascota.Cliente.Apellido != null && t.Mascota.Cliente.Apellido.Contains(buscar)) ||
                (t.Motivo != null && t.Motivo.Contains(buscar)) ||
                (t.Observacion != null && t.Observacion.Contains(buscar)));
        }

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(t => t.EstadoTurno == estado);

        if (fecha.HasValue)
        {
            var dia = fecha.Value.Date;
            query = query.Where(t => t.Fecha.HasValue && t.Fecha.Value.Date == dia);
        }

        query = query.OrderByDescending(t => t.Fecha)
                     .ThenBy(t => t.HoraTurno);

        ViewBag.Buscar = buscar;
        ViewBag.Estado = estado;
        ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd");
        ViewBag.EstadosTurno = new List<string>
        {
            EstadoTurno.Pendiente,
            EstadoTurno.Confirmado,
            EstadoTurno.Atendido,
            EstadoTurno.Cancelado
        };

        return View(await PaginatedList<Turno>.CreateFromQueryAsync(query, pagina, PageSize));
    }

    // GET: /Turnos/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var turno = await _context.Turnos
                                  .Include(t => t.Mascota)
                                      .ThenInclude(m => m.Cliente)
                                  .Include(t => t.Mascota)
                                      .ThenInclude(m => m.Especie)
                                  .Include(t => t.Historial)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(t => t.IdTurno == id);
        if (turno is null) return NotFound();
        return View(turno);
    }

    // GET: /Turnos/Crear
    public async Task<IActionResult> Crear(int? idMascota = null)
    {
        await CargarSelectsAsync(idMascota);
        return View(new Turno
        {
            IdMascota = idMascota ?? 0,
            Fecha = DateTime.Today,
            EstadoTurno = EstadoTurno.Pendiente
        });
    }

    // POST: /Turnos/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Turno turno)
    {
        ModelState.Remove(nameof(Turno.Mascota));
        ModelState.Remove(nameof(Turno.Historial));

        var horaNormalizada = NormalizarHora(turno.HoraTurno);
        if (!turno.Fecha.HasValue)
            ModelState.AddModelError("Fecha", "La fecha es obligatoria.");
        if (horaNormalizada is null)
            ModelState.AddModelError("HoraTurno", "La hora debe tener formato HH:mm.");

        bool mascotaExiste = await _context.Mascotas
            .AnyAsync(m => m.IdMascota == turno.IdMascota && m.Estado == 1);
        if (!mascotaExiste)
            ModelState.AddModelError("IdMascota", "Debe seleccionar una mascota activa.");

        if (turno.Fecha.HasValue && horaNormalizada is not null &&
            await ExisteConflictoHorarioAsync(turno.Fecha.Value.Date, horaNormalizada))
        {
            ModelState.AddModelError("HoraTurno",
                "Ya existe un turno asignado para ese dia y horario.");
        }

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(turno.IdMascota);
            return View(turno);
        }

        turno.HoraTurno = horaNormalizada!;
        turno.Estado = 1;
        turno.EstadoTurno ??= EstadoTurno.Pendiente;

        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Turno registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Turnos/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var turno = await _context.Turnos.FindAsync(id);
        if (turno is null) return NotFound();
        await CargarSelectsAsync(turno.IdMascota);
        return View(turno);
    }

    // POST: /Turnos/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Turno turno)
    {
        if (id != turno.IdTurno) return BadRequest();

        ModelState.Remove(nameof(Turno.Mascota));
        ModelState.Remove(nameof(Turno.Historial));

        var horaNormalizada = NormalizarHora(turno.HoraTurno);
        if (!turno.Fecha.HasValue)
            ModelState.AddModelError("Fecha", "La fecha es obligatoria.");
        if (horaNormalizada is null)
            ModelState.AddModelError("HoraTurno", "La hora debe tener formato HH:mm.");

        bool mascotaExiste = await _context.Mascotas
            .AnyAsync(m => m.IdMascota == turno.IdMascota && m.Estado == 1);
        if (!mascotaExiste)
            ModelState.AddModelError("IdMascota", "Debe seleccionar una mascota activa.");

        if (turno.Fecha.HasValue && horaNormalizada is not null &&
            await ExisteConflictoHorarioAsync(turno.Fecha.Value.Date, horaNormalizada, turno.IdTurno))
        {
            ModelState.AddModelError("HoraTurno",
                "Ya existe un turno asignado para ese dia y horario.");
        }

        if (!ModelState.IsValid)
        {
            await CargarSelectsAsync(turno.IdMascota);
            return View(turno);
        }

        turno.HoraTurno = horaNormalizada!;

        var entry = _context.ChangeTracker.Entries<Turno>().FirstOrDefault();
        if (entry != null) entry.State = EntityState.Detached;

        _context.Entry(turno).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Turno actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Turnos/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var turno = await _context.Turnos
                                  .Include(t => t.Historial)
                                  .FirstOrDefaultAsync(t => t.IdTurno == id);
        if (turno is null) return NotFound();

        turno.Estado = 0;
        turno.EstadoTurno = EstadoTurno.Cancelado;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Turno cancelado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ExisteConflictoHorarioAsync(DateTime fecha, string hora, int? excluirId = null)
    {
        var query = _context.Turnos
            .Where(t => t.Estado == 1 &&
                        t.EstadoTurno != EstadoTurno.Cancelado &&
                        t.Fecha.HasValue &&
                        t.Fecha.Value.Date == fecha &&
                        t.HoraTurno == hora);

        if (excluirId.HasValue)
            query = query.Where(t => t.IdTurno != excluirId.Value);

        return await query.AnyAsync();
    }

    private async Task CargarSelectsAsync(int? idMascotaSeleccionada = null)
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

        ViewBag.Mascotas = new SelectList(mascotas, "IdMascota", "Texto", idMascotaSeleccionada);
        ViewBag.EstadosTurno = new List<string>
        {
            EstadoTurno.Pendiente,
            EstadoTurno.Confirmado,
            EstadoTurno.Atendido,
            EstadoTurno.Cancelado
        };
    }

    private string? NormalizarHora(string? hora)
    {
        if (string.IsNullOrWhiteSpace(hora)) return null;
        if (!TimeSpan.TryParse(hora.Trim(), out var ts)) return null;
        return ts.ToString(@"hh\:mm");
    }
}
