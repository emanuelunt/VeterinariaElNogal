using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Tipo
{
    public int IdTipo { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(100)]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Estado")]
    public int Estado { get; set; } = 1;

    // Navegación
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}