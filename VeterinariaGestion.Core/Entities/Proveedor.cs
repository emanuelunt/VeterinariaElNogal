using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Proveedor
{
    public int IdProveedor { get; set; }

    [Required(ErrorMessage = "La razón social es obligatoria")]
    [StringLength(200)]
    [Display(Name = "Razón Social")]
    public string RazonSocial { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "CUIL/CUIT")]
    public string? CuilCuit { get; set; }

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

    // Navegación
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}