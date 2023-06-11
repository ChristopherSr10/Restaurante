using System.ComponentModel.DataAnnotations;

namespace pruebarestaurante.Models
{
    public class PlatilloIngredienteViewModel
    {
         [Key] public int idPlatillo { get; set; }
        [Required]
    public string nombrePlatillo { get; set; }
    [Required]
    public float precioPlatillo { get; set; }
    [Required]
    public string descripcionPlatillo { get; set; }
    [Display(Name = "Número de Ingredientes")]
    public int numeroIngredientes { get; set; }

    public List<IngredienteViewModel> Ingredientes { get; set; }
    }

    public class IngredienteViewModel
{
    public int idIngrediente { get; set; }
    [Required]
    public string nombreIngrediente { get; set; }
    [Required]
    public int cantidadDisponible { get; set; }
}
}
