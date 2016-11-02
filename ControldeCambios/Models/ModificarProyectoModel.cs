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
        [RegularExpression(@"^[A-Za-z0-9\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        public string nombre { get; set; }

        [Display(Name = "Descripción")]
        public string descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es un campo requerido.")]
        [Display(Name = "Fecha de Inicio")]
        public string fechaInicio { get; set; }

        [Display(Name = "Fecha Final")]
        [Required(ErrorMessage = "La fecha final es un campo requerido.")]
        public string fechaFinal { get; set; }

        [Display(Name = "Líder")]
        [Required(ErrorMessage = "El líder es un campo requerido.")]
        public Usuario lider { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "El cliente es un campo requerido.")]
        public Usuario cliente { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado del proyecto es un campo requerido.")]
        public string estado { get; set; }

        [Display(Name = "Equipo")]
        public List<string> equipo { get; set; }
    }
}