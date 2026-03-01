using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly VeterinariaDbContext _context;
    private const int PageSize = 10;

    public UsuariosController(VeterinariaDbContext context)
        => _context = context;

    // GET: /Usuarios
    public async Task<IActionResult> Index(string? buscar, int pagina = 1)
    {
        var query = _context.Usuarios
                            .AsNoTracking()
                            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(u =>
                u.NombreUsuario.Contains(buscar) ||
                u.Email.Contains(buscar));
        }

        query = query.OrderBy(u => u.NombreUsuario);

        ViewBag.Buscar = buscar;
        return View(await PaginatedList<Usuario>.CreateFromQueryAsync(query, pagina, PageSize));
    }

    // GET: /Usuarios/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var usuario = await _context.Usuarios
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.IdUsuario == id);
        if (usuario is null) return NotFound();
        return View(usuario);
    }

    // GET: /Usuarios/Crear
    public IActionResult Crear() => View(new Usuario());

    // POST: /Usuarios/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Usuario usuario, string password, string confirmarPassword)
    {
        ModelState.Remove(nameof(Usuario.PasswordHash));

        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError("PasswordHash", "La contraseña es obligatoria.");
        if (password.Length < 6)
            ModelState.AddModelError("PasswordHash", "La contraseña debe tener al menos 6 caracteres.");
        if (password != confirmarPassword)
            ModelState.AddModelError("PasswordHash", "La confirmación de contraseña no coincide.");

        bool existeUsuario = await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario);
        if (existeUsuario)
            ModelState.AddModelError("NombreUsuario", "Ya existe un usuario con ese nombre.");

        bool existeEmail = await _context.Usuarios
            .AnyAsync(u => u.Email == usuario.Email);
        if (existeEmail)
            ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");

        if (!ModelState.IsValid) return View(usuario);

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        usuario.FechaCreacion = DateTime.Now;
        usuario.Activo = 1;

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Usuario registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Usuarios/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();
        return View(usuario);
    }

    // POST: /Usuarios/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
        int id,
        Usuario usuario,
        string? nuevaPassword,
        string? confirmarPassword)
    {
        if (id != usuario.IdUsuario) return BadRequest();

        ModelState.Remove(nameof(Usuario.PasswordHash));

        bool existeUsuario = await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario &&
                           u.IdUsuario != id);
        if (existeUsuario)
            ModelState.AddModelError("NombreUsuario", "Ya existe otro usuario con ese nombre.");

        bool existeEmail = await _context.Usuarios
            .AnyAsync(u => u.Email == usuario.Email &&
                           u.IdUsuario != id);
        if (existeEmail)
            ModelState.AddModelError("Email", "Ya existe otro usuario con ese email.");

        if (!string.IsNullOrWhiteSpace(nuevaPassword))
        {
            if (nuevaPassword.Length < 6)
                ModelState.AddModelError("PasswordHash", "La nueva contraseña debe tener al menos 6 caracteres.");
            if (nuevaPassword != (confirmarPassword ?? string.Empty))
                ModelState.AddModelError("PasswordHash", "La confirmación de contraseña no coincide.");
        }

        if (!ModelState.IsValid) return View(usuario);

        var actual = await _context.Usuarios.FindAsync(id);
        if (actual is null) return NotFound();

        actual.NombreUsuario = usuario.NombreUsuario;
        actual.Email = usuario.Email;
        actual.Activo = usuario.Activo;

        if (!string.IsNullOrWhiteSpace(nuevaPassword))
            actual.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword, workFactor: 11);

        await _context.SaveChangesAsync();

        TempData["Exito"] = "Usuario actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Usuarios/CambiarEstado/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Activo = usuario.Activo == 1 ? 0 : 1;
        await _context.SaveChangesAsync();

        string estado = usuario.Activo == 1 ? "activado" : "desactivado";
        TempData["Exito"] = $"Usuario {estado} correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Usuarios/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Activo = 0;
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Usuario desactivado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
