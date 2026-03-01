using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Cliente
{
    public int IdCliente { get; set; }

    [StringLength(20)]
    [Display(Name = "CUIL/DNI")]
    public string? CuilDni { get; set; }

    [StringLength(100)]
    [Display(Name = "Apellido")]
    public string? Apellido { get; set; }

    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [StringLength(250)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Fecha de Alta")]
    public DateTime? FechaAlta { get; set; } = DateTime.Now;

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Propiedad calculada
    public string NombreCompleto => $"{Apellido}, {Nombre}".Trim(',').Trim();

    // Navegación
    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<CuentaCorriente> CuentasCorrientes { get; set; } = new List<CuentaCorriente>();
}