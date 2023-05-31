using System.Security.AccessControl;
using System.ComponentModel.DataAnnotations;
namespace pruebarestaurante.Models
{
    public class PlatilloViewModel
    {
        [Key] public int idPlatillo { get; set; }
        [Required] public string nombrePlatillo { get; set; }
        [Required] public float precioPlatillo { get; set; }
        [Required] public string descripcionPlatillo { get; set; }
    }
}

