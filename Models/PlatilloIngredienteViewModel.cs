using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pruebarestaurante.Models
{
    public class PlatilloIngredienteViewModel
    {
        [Key]
        public int idPlatillo { get; set; }

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

    public class Ordenes
    {
        public int idOrden { get; set; }
        public int cantidadOrdenPlatillo { get; set; }
        public int precioPlatillo { get; set; }
        public int costoPlatillos { get; set; }

    }
    public class Usuario
    {
        public string username { get; set; }
        public string password { get; set; }

    }
    public class Orden
    {
        public int CantidadOrdenPlatillo { get; set; }
        public string NombrePlatillo { get; set; }
        public decimal PrecioPlatillo { get; set; }
        public int IdOrden { get; set; }
        public int IdPlatillo { get; set; }
        public int CostoPlatillos { get; set; }

    }

}
