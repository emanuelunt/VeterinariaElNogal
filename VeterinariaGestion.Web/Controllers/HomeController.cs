using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeterinariaGestion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VeterinariaGestion.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly VeterinariaDbContext _context;

    public HomeController(VeterinariaDbContext context)
        => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalClientes  = await _context.Clientes.CountAsync(c => c.Estado == 1);
        ViewBag.TotalProductos = await _context.Productos.CountAsync(p => p.Estado == 1);
        ViewBag.TotalMascotas  = await _context.Mascotas.CountAsync(m => m.Estado == 1);
        ViewBag.TotalVentas    = await _context.Ventas.CountAsync(v => v.Estado == 1);
        ViewBag.StockBajo      = await _context.Productos
                                               .CountAsync(p => p.Estado == 1 && p.Stock <= 5);
        ViewBag.TurnosPendientes = await _context.Turnos
                                                 .CountAsync(t => t.Estado == 1 &&
                                                             t.EstadoTurno == "Pendiente");
        return View();
    }
}