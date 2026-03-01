using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;
using VeterinariaGestion.Infrastructure.Repositories;

namespace VeterinariaGestion.Infrastructure.Services;

public interface IVentaService
{
    Task<IEnumerable<Venta>> ObtenerTodosAsync();
    Task<Venta?> ObtenerConDetallesAsync(int id);
    Task<IEnumerable<Venta>> ObtenerPorClienteAsync(int idCliente);
    Task<(bool exito, string mensaje, int idVenta)> CrearVentaAsync(
        Venta venta, List<VentaDetalle> detalles, int cantidadCuotas = 1);
    Task<(bool exito, string mensaje)> AnularVentaAsync(int id);
}

public class VentaService : IVentaService
{
    private readonly IVentaRepository    _ventaRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly VeterinariaDbContext _context;

    public VentaService(
        IVentaRepository ventaRepo,
        IProductoRepository productoRepo,
        VeterinariaDbContext context)
    {
        _ventaRepo    = ventaRepo;
        _productoRepo = productoRepo;
        _context      = context;
    }

    public Task<IEnumerable<Venta>> ObtenerTodosAsync()
        => _ventaRepo.GetAllAsync();

    public Task<Venta?> ObtenerConDetallesAsync(int id)
        => _ventaRepo.GetConDetallesAsync(id);

    public Task<IEnumerable<Venta>> ObtenerPorClienteAsync(int idCliente)
        => _ventaRepo.GetByClienteAsync(idCliente);

    public async Task<(bool exito, string mensaje, int idVenta)> CrearVentaAsync(
        Venta venta, List<VentaDetalle> detalles, int cantidadCuotas = 1)
    {
        // Verificar stock
        foreach (var detalle in detalles)
        {
            var producto = await _productoRepo.GetByIdAsync(detalle.IdProducto);
            if (producto is null)
                return (false, $"Producto ID {detalle.IdProducto} no encontrado.", 0);
            if (producto.Stock < detalle.Cantidad)
                return (false, $"Stock insuficiente para '{producto.Nombre}'. " +
                               $"Disponible: {producto.Stock}.", 0);
        }

        // Calcular subtotales
        foreach (var detalle in detalles)
            detalle.SubTotalItem = (detalle.PrecioUnitario * detalle.Cantidad) - detalle.DescuentoItem;

        venta.SubTotal    = detalles.Sum(d => d.SubTotalItem);
        venta.Total       = venta.SubTotal - venta.Descuento + venta.Recargo;
        venta.NumeroVenta = await _ventaRepo.GenerarNumeroVentaAsync();
        venta.Fecha       = DateTime.Now;
        venta.Estado      = 1;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Ventas.AddAsync(venta);
            await _context.SaveChangesAsync();

            foreach (var detalle in detalles)
            {
                detalle.IdVenta = venta.IdVenta;
                await _context.VentasDetalle.AddAsync(detalle);
                await _productoRepo.DescontarStockAsync(detalle.IdProducto, detalle.Cantidad);
            }
            await _context.SaveChangesAsync();

            // Generar cuotas
            if (cantidadCuotas > 1)
            {
                decimal montoPorCuota = Math.Round(venta.Total / cantidadCuotas, 2);
                for (int i = 1; i <= cantidadCuotas; i++)
                {
                    await _context.Cuotas.AddAsync(new Cuota
                    {
                        IdVenta          = venta.IdVenta,
                        NumeroCuota      = i,
                        MontoCuota       = montoPorCuota,
                        MontoPagado      = 0,
                        SaldoPendiente   = montoPorCuota,
                        FechaVencimiento = DateTime.Now.AddMonths(i),
                        EstadoCuota      = "Pendiente",
                        Estado           = 1
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Cuenta corriente
            if (venta.FormaPago == FormaPago.CuentaCorriente && venta.IdCliente.HasValue)
            {
                var ultimo = await _context.CuentasCorrientes
                    .Where(cc => cc.IdCliente == venta.IdCliente)
                    .OrderByDescending(cc => cc.IdCuentaCorriente)
                    .FirstOrDefaultAsync();

                decimal saldoAnterior = ultimo?.SaldoNuevo ?? 0;

                await _context.CuentasCorrientes.AddAsync(new CuentaCorriente
                {
                    IdCliente       = venta.IdCliente.Value,
                    IdVenta         = venta.IdVenta,
                    FechaMovimiento = DateTime.Now,
                    TipoMovimiento  = "Débito",
                    Concepto        = $"Venta {venta.NumeroVenta}",
                    Importe         = venta.Total,
                    SaldoAnterior   = saldoAnterior,
                    SaldoNuevo      = saldoAnterior + venta.Total,
                    EstadoCuenta    = "Pendiente",
                    Estado          = 1
                });
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return (true, "Venta registrada correctamente.", venta.IdVenta);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al registrar la venta: {ex.Message}", 0);
        }
    }

    public async Task<(bool exito, string mensaje)> AnularVentaAsync(int id)
    {
        var venta = await _context.Ventas
                                  .Include(v => v.Detalles)
                                  .FirstOrDefaultAsync(v => v.IdVenta == id);
        if (venta is null) return (false, "La venta no existe.");
        if (venta.EstadoPago == EstadoPago.Anulado) return (false, "Ya está anulada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.IdProducto);
                if (producto is not null) producto.Stock += detalle.Cantidad;
            }
            venta.EstadoPago = EstadoPago.Anulado;
            venta.Estado     = 0;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Venta anulada correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al anular: {ex.Message}");
        }
    }
}