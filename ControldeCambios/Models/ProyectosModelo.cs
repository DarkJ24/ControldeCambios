using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ControldeCambios.Models
{
    /// <summary>
    /// Provee modelado para los objetos de tipo Proyecto
    /// </summary>
    public class ProyectosModelo
    {
        public Proyecto proyecto { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [StringLength(25, ErrorMessage = "El nombre debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El nombre debe ser de almenos {1} dígitos")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        public string nombre { get; set; }

        [Display(Name = "Descripción")]
        public string descripcion { get; set; }

        public DateTime fechaInicio { get; set; }

        public DateTime fechaFinal { get; set; }

        public Usuario lider { get; set; }

        public Usuario cliente { get; set; }

        public List<Usuario> equipo { get; set; }

    }
}