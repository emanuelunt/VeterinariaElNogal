using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using VeterinariaGestion.Core.Entities;
using VeterinariaGestion.Infrastructure.Data;

namespace VeterinariaGestion.Infrastructure.Services;

public interface IAuthService
{
    Task<Usuario?> LoginAsync(string username, string password);
    Task<(bool exito, string mensaje)> RegistrarAsync(string username, string email, string password);
    string HashPassword(string password);
}

public class AuthService : IAuthService
{
    private readonly VeterinariaDbContext _context;

    public AuthService(VeterinariaDbContext context) => _context = context;

    public async Task<Usuario?> LoginAsync(string username, string password)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == username && u.Activo == 1);
        if (usuario is null) return null;
        return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash) ? usuario : null;
    }

    public async Task<(bool exito, string mensaje)> RegistrarAsync(
        string username, string email, string password)
    {
        bool existe = await _context.Usuarios
            .AnyAsync(u => u.NombreUsuario == username || u.Email == email);
        if (existe) return (false, "El usuario o email ya están registrados.");

        var usuario = new Usuario
        {
            NombreUsuario = username,
            Email         = email,
            PasswordHash  = HashPassword(password),
            FechaCreacion = DateTime.Now,
            Activo        = 1
        };
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
        return (true, "Usuario registrado correctamente.");
    }

    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
}