using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Historial
{
    public int IdHistorial { get; set; }

    [Display(Name = "Mascota")]
    public int IdMascota { get; set; }

    [Display(Name = "Turno")]
    public int? IdTurno { get; set; }

    [StringLength(500)]
    [Display(Name = "Motivo de Consulta")]
    public string? MotivoConsulta { get; set; }

    [StringLength(1000)]
    [Display(Name = "Diagnóstico")]
    public string? Diagnostico { get; set; }

    [StringLength(1000)]
    [Display(Name = "Tratamiento")]
    public string? Tratamiento { get; set; }

    [StringLength(1000)]
    [Display(Name = "Indicaciones")]
    public string? Indicacion { get; set; }

    [StringLength(500)]
    [Display(Name = "Observación")]
    public string? Observacion { get; set; }

    [Display(Name = "Fecha de Consulta")]
    public DateTime? FechaConsulta { get; set; } = DateTime.Now;

    [Display(Name = "Próximo Control")]
    public DateTime? ProximoControl { get; set; }

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public Mascota Mascota { get; set; } = null!;
    public Turno? Turno { get; set; }
}