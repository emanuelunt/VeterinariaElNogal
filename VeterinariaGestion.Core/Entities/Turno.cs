using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Turno
{
    public int IdTurno { get; set; }

    [Display(Name = "Fecha")]
    public DateTime? Fecha { get; set; }

    [StringLength(50)]
    [Display(Name = "Estado del Turno")]
    public string? EstadoTurno { get; set; } = "Pendiente";

    [StringLength(10)]
    [Display(Name = "Hora")]
    public string? HoraTurno { get; set; }

    [StringLength(250)]
    [Display(Name = "Motivo")]
    public string? Motivo { get; set; }

    [Display(Name = "Mascota")]
    public int IdMascota { get; set; }

    [StringLength(500)]
    [Display(Name = "Observación")]
    public string? Observacion { get; set; }

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public Mascota Mascota { get; set; } = null!;
    public Historial? Historial { get; set; }
}

public static class EstadoTurno
{
    public const string Pendiente  = "Pendiente";
    public const string Confirmado = "Confirmado";
    public const string Atendido   = "Atendido";
    public const string Cancelado  = "Cancelado";
}