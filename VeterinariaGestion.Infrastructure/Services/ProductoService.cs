using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Repositories;

namespace VeterinariaGestion.Infrastructure.Services;

public interface IProductoService
{
    Task<IEnumerable<Producto>> ObtenerTodosAsync();
    Task<IEnumerable<Producto>> ObtenerActivosAsync();
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Producto>> ObtenerStockBajoAsync(int minimo = 5);
    Task<(bool exito, string mensaje)> CrearAsync(Producto producto);
    Task<(bool exito, string mensaje)> ActualizarAsync(Producto producto);
    Task<(bool exito, string mensaje)> EliminarAsync(int id);
}

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repo;

    public ProductoService(IProductoRepository repo) => _repo = repo;

    public Task<IEnumerable<Producto>> ObtenerTodosAsync()   => _repo.GetAllAsync();
    public Task<IEnumerable<Producto>> ObtenerActivosAsync() => _repo.GetActivosAsync();
    public Task<Producto?> ObtenerPorIdAsync(int id)         => _repo.GetByIdAsync(id);
    public Task<IEnumerable<Producto>> ObtenerStockBajoAsync(int minimo = 5)
        => _repo.GetConStockBajoAsync(minimo);

    public async Task<(bool exito, string mensaje)> CrearAsync(Producto producto)
    {
        await _repo.AddAsync(producto);
        return (true, "Producto registrado correctamente.");
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(Producto producto)
    {
        if (!await _repo.ExistsAsync(producto.IdProducto))
            return (false, "El producto no existe.");
        await _repo.UpdateAsync(producto);
        return (true, "Producto actualizado correctamente.");
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        var producto = await _repo.GetByIdAsync(id);
        if (producto is null) return (false, "El producto no existe.");
        producto.Estado = 0;
        await _repo.UpdateAsync(producto);
        return (true, "Producto eliminado correctamente.");
    }
}