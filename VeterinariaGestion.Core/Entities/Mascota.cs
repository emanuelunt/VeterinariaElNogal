using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Mascota
{
    public int IdMascota { get; set; }

    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Display(Name = "Cliente")]
    public int IdCliente { get; set; }

    [Display(Name = "Especie")]
    public int IdEspecie { get; set; }

    [StringLength(10)]
    [Display(Name = "Sexo")]
    public string? Sexo { get; set; }

    [Display(Name = "Fecha de Nacimiento")]
    public DateTime? FechaNaci { get; set; }

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public Cliente Cliente { get; set; } = null!;
    public Especie Especie { get; set; } = null!;
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
    public ICollection<Historial> Historiales { get; set; } = new List<Historial>();
}