using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Producto
{
    public int IdProducto { get; set; }

    [StringLength(50)]
    [Display(Name = "Código")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Stock")]
    public int Stock { get; set; } = 0;

    [Display(Name = "Precio Minorista")]
    [DataType(DataType.Currency)]
    public decimal PrecioMinorista { get; set; }

    [Display(Name = "Precio Mayorista")]
    [DataType(DataType.Currency)]
    public decimal PrecioMayorista { get; set; }

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Display(Name = "Tipo")]
    public int? IdTipo { get; set; }

    [Display(Name = "Proveedor")]
    public int? IdProveedor { get; set; }

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public Tipo? Tipo { get; set; }
    public Proveedor? Proveedor { get; set; }
    public ICollection<VentaDetalle> VentaDetalles { get; set; } = new List<VentaDetalle>();
}