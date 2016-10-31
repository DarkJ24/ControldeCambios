using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Models
{
    /// <summary>
    /// Provee modelado para los objetos de tipo Proyecto
    /// </summary>
    public class ModificarProyectoModel
    {

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [StringLength(25, ErrorMessage = "El nombre debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El nombre debe ser de almenos {1} dígitos")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        public string nombre { get; set; }

        [Display(Name = "Descripción")]
        public string descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es un campo requerido.")]
        [Display(Name = "Fecha de Inicio")]
        public string fechaInicio { get; set; }

        [Display(Name = "Fecha Final")]
        public string fechaFinal { get; set; }

        [Display(Name = "Líder")]
        public Usuario lider { get; set; }

        [Display(Name = "Cliente")]
        public Usuario cliente { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "Se requiere un estado")]
        public string estado { get; set; }

        [Display(Name = "Equipo")]
        public List<string> equipo { get; set; }
    }
}