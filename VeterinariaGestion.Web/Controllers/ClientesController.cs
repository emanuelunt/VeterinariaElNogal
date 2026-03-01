using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Infrastructure.Services;
using VeterinariaGestion.Web.Models;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly IClienteService _service;
    private readonly VeterinariaDbContext _context;    
    private const int PageSize = 10;

    public ClientesController(IClienteService service, VeterinariaDbContext context)
        => (_service, _context) = (service, context);

    // GET: /Clientes
    /*  public async Task<IActionResult> Index(string? buscar, int pagina = 1)
     {
         var todos = await _service.ObtenerTodosAsync();

         if (!string.IsNullOrWhiteSpace(buscar))
         {
             buscar = buscar.Trim();
             todos  = todos.Where(c =>
                 (c.Nombre   != null && c.Nombre.Contains(buscar,   StringComparison.OrdinalIgnoreCase)) ||
                 (c.Apellido != null && c.Apellido.Contains(buscar, StringComparison.OrdinalIgnoreCase)) ||
                 (c.CuilDni  != null && c.CuilDni.Contains(buscar,  StringComparison.OrdinalIgnoreCase)) ||
                 (c.Telefono != null && c.Telefono.Contains(buscar, StringComparison.OrdinalIgnoreCase)) ||
                 (c.Email    != null && c.Email.Contains(buscar,    StringComparison.OrdinalIgnoreCase)));
         }

         ViewBag.Buscar = buscar;

         var paginado = PaginatedList<Cliente>.Create(todos, pagina, PageSize);
         return View(paginado);
     } */

    public async Task<IActionResult> Index(string? buscar, int pagina = 1) // Versión optimizada para paginación en base de datos
    {
        var query = _context.Clientes
                            .Where(c => c.Estado == 1)
                            .AsNoTracking()
                            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            buscar = buscar.Trim();
            query = query.Where(c =>
                (c.Nombre != null && c.Nombre.Contains(buscar)) ||
                (c.Apellido != null && c.Apellido.Contains(buscar)) ||
                (c.CuilDni != null && c.CuilDni.Contains(buscar)) ||
                (c.Telefono != null && c.Telefono.Contains(buscar)) ||
                (c.Email != null && c.Email.Contains(buscar)));
        }

        query = query.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre);

        ViewBag.Buscar = buscar;
        return View(await PaginatedList<Cliente>.CreateFromQueryAsync(
            query, pagina, PageSize));
    }

    // GET: /Clientes/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var cliente = await _service.ObtenerConMascotasAsync(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    // GET: /Clientes/Crear
    public IActionResult Crear() => View();

    // POST: /Clientes/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Cliente cliente)
    {
        if (!ModelState.IsValid) return View(cliente);
        var (exito, mensaje) = await _service.CrearAsync(cliente);
        TempData[exito ? "Exito" : "Error"] = mensaje;
        return exito ? RedirectToAction(nameof(Index)) : View(cliente);
    }

    // GET: /Clientes/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        var cliente = await _service.ObtenerPorIdAsync(id);
        if (cliente is null) return NotFound();
        return View(cliente);
    }

    // POST: /Clientes/Editar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Cliente cliente)
    {
        if (id != cliente.IdCliente) return BadRequest();
        if (!ModelState.IsValid) return View(cliente);
        var (exito, mensaje) = await _service.ActualizarAsync(cliente);
        TempData[exito ? "Exito" : "Error"] = mensaje;
        return RedirectToAction(nameof(Index));
    }

    // POST: /Clientes/CambiarEstado/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var cliente = await _service.ObtenerPorIdAsync(id);
        if (cliente is null) return NotFound();

        cliente.Estado = cliente.Estado == 1 ? 0 : 1;
        await _service.ActualizarAsync(cliente);

        string estado = cliente.Estado == 1 ? "activado" : "desactivado";
        TempData["Exito"] = $"Cliente {estado} correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Clientes/Eliminar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var (exito, mensaje) = await _service.EliminarAsync(id);
        TempData[exito ? "Exito" : "Error"] = mensaje;
        return RedirectToAction(nameof(Index));
    }
}