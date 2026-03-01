using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Cuota
{
    public int IdCuota { get; set; }

    [Display(Name = "N° Cuota")]
    public int NumeroCuota { get; set; }

    [Display(Name = "Vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    [Display(Name = "Fecha de Pago")]
    public DateTime? FechaPago { get; set; }

    [Display(Name = "Monto Cuota")]
    [DataType(DataType.Currency)]
    public decimal MontoCuota { get; set; }

    [Display(Name = "Monto Pagado")]
    [DataType(DataType.Currency)]
    public decimal MontoPagado { get; set; }

    [Display(Name = "Saldo Pendiente")]
    [DataType(DataType.Currency)]
    public decimal SaldoPendiente { get; set; }

    [Display(Name = "Venta")]
    public int IdVenta { get; set; }

    [Display(Name = "Interés Mora")]
    [DataType(DataType.Currency)]
    public decimal InteresMora { get; set; }

    [StringLength(50)]
    [Display(Name = "Estado Cuota")]
    public string? EstadoCuota { get; set; } = "Pendiente";

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public Venta Venta { get; set; } = null!;
}