using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class CuentaCorriente
{
    public int IdCuentaCorriente { get; set; }

    [Display(Name = "Cliente")]
    public int IdCliente { get; set; }

    [Display(Name = "Venta")]
    public int? IdVenta { get; set; }

    [Display(Name = "Fecha Movimiento")]
    public DateTime? FechaMovimiento { get; set; } = DateTime.Now;

    [StringLength(50)]
    [Display(Name = "Tipo Movimiento")]
    public string? TipoMovimiento { get; set; }

    [StringLength(250)]
    [Display(Name = "Concepto")]
    public string? Concepto { get; set; }

    [Display(Name = "Importe")]
    [DataType(DataType.Currency)]
    public decimal Importe { get; set; }

    [Display(Name = "Saldo Anterior")]
    [DataType(DataType.Currency)]
    public decimal SaldoAnterior { get; set; }

    [Display(Name = "Saldo Nuevo")]
    [DataType(DataType.Currency)]
    public decimal SaldoNuevo { get; set; }

    [Display(Name = "Vencimiento")]
    public DateTime? FechaVencimiento { get; set; }

    [StringLength(50)]
    [Display(Name = "Estado Cuenta")]
    public string? EstadoCuenta { get; set; }

    [StringLength(100)]
    [Display(Name = "Comprobante")]
    public string? Comprobante { get; set; }

    [StringLength(500)]
    [Display(Name = "Observación")]
    public string? Observacion { get; set; }

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public Venta? Venta { get; set; }
}