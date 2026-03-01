using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Repositories;

namespace VeterinariaGestion.Infrastructure.Services;

public interface IClienteService
{
    Task<IEnumerable<Cliente>> ObtenerTodosAsync();
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<Cliente?> ObtenerConMascotasAsync(int id);
    Task<IEnumerable<Cliente>> BuscarAsync(string termino);
    Task<(bool exito, string mensaje)> CrearAsync(Cliente cliente);
    Task<(bool exito, string mensaje)> ActualizarAsync(Cliente cliente);
    Task<(bool exito, string mensaje)> EliminarAsync(int id);
}

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repo;

    public ClienteService(IClienteRepository repo) => _repo = repo;

    public Task<IEnumerable<Cliente>> ObtenerTodosAsync()   => _repo.GetAllAsync();
    public Task<Cliente?> ObtenerPorIdAsync(int id)         => _repo.GetByIdAsync(id);
    public Task<Cliente?> ObtenerConMascotasAsync(int id)   => _repo.GetConMascotasAsync(id);
    public Task<IEnumerable<Cliente>> BuscarAsync(string t) => _repo.BuscarAsync(t);

    public async Task<(bool exito, string mensaje)> CrearAsync(Cliente cliente)
    {
        cliente.FechaAlta = DateTime.Now;
        await _repo.AddAsync(cliente);
        return (true, "Cliente registrado correctamente.");
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(Cliente cliente)
    {
        if (!await _repo.ExistsAsync(cliente.IdCliente))
            return (false, "El cliente no existe.");
        await _repo.UpdateAsync(cliente);
        return (true, "Cliente actualizado correctamente.");
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        var cliente = await _repo.GetByIdAsync(id);
        if (cliente is null) return (false, "El cliente no existe.");
        cliente.Estado = 0;
        await _repo.UpdateAsync(cliente);
        return (true, "Cliente eliminado correctamente.");
    }
}