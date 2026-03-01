using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Venta
{
    public int IdVenta { get; set; }

    [StringLength(50)]
    [Display(Name = "Número de Venta")]
    public string? NumeroVenta { get; set; }

    [Display(Name = "Fecha")]
    public DateTime? Fecha { get; set; } = DateTime.Now;

    [Display(Name = "Cliente")]
    public int? IdCliente { get; set; }

    [Display(Name = "Subtotal")]
    [DataType(DataType.Currency)]
    public decimal SubTotal { get; set; }

    [Display(Name = "Descuento")]
    [DataType(DataType.Currency)]
    public decimal Descuento { get; set; }

    [Display(Name = "Recargo")]
    [DataType(DataType.Currency)]
    public decimal Recargo { get; set; }

    [Display(Name = "Total")]
    [DataType(DataType.Currency)]
    public decimal Total { get; set; }

    [StringLength(50)]
    [Display(Name = "Forma de Pago")]
    public string? FormaPago { get; set; }

    [StringLength(50)]
    [Display(Name = "Estado de Pago")]
    public string? EstadoPago { get; set; }

    [StringLength(500)]
    [Display(Name = "Observación")]
    public string? Observacion { get; set; }

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public Cliente? Cliente { get; set; }
    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
    public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
    public ICollection<CuentaCorriente> CuentasCorrientes { get; set; } = new List<CuentaCorriente>();
}

public static class FormaPago
{
    public const string Efectivo      = "Efectivo";
    public const string Tarjeta       = "Tarjeta";
    public const string Transferencia = "Transferencia";
    public const string CuentaCorriente = "Cuenta Corriente";
}

public static class EstadoPago
{
    public const string Pendiente = "Pendiente";
    public const string Pagado    = "Pagado";
    public const string Parcial   = "Parcial";
    public const string Anulado   = "Anulado";
}