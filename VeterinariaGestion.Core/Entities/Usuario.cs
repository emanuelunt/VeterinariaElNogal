using System.ComponentModel.DataAnnotations;

namespace VeterinariaGestion.Core.Entities;

public class Usuario
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    public string? PasswordHash { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime? FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Activo")]
    public int Activo { get; set; } = 1;
}