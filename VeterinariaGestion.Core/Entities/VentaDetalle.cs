using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class VentaDetalle
{
    public int IdVentaDetalle { get; set; }

    [Display(Name = "Venta")]
    public int IdVenta { get; set; }

    [Display(Name = "Producto")]
    public int IdProducto { get; set; }

    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; }

    [Display(Name = "Precio Unitario")]
    [DataType(DataType.Currency)]
    public decimal PrecioUnitario { get; set; }

    [Display(Name = "Descuento Item")]
    [DataType(DataType.Currency)]
    public decimal DescuentoItem { get; set; } = 0;

    [Display(Name = "Subtotal Item")]
    [DataType(DataType.Currency)]
    public decimal SubTotalItem { get; set; }

    // Navegación
    public Venta Venta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}