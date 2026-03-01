using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Especie
{
    public int IdEspecie { get; set; }

    [StringLength(100)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}