using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Models
{
    public class SprintModelo
    {

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [StringLength(25, ErrorMessage = "El nombre debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El nombre debe ser de almenos {1} dígitos")]
        [RegularExpression(@"^[A-Za-z0-9\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        [Display(Name = "Proyecto")]
        public string proyecto { get; set; }


        [Required(ErrorMessage = "El número es un campo requerido.")]
        [StringLength(15, ErrorMessage = "El número debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El número debe ser de al menos {1} dígito")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El número no puede contener letras, espacios o guiones")]
        [Display(Name = "Numero")]
        public int numero { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es un campo requerido.")]
        [Display(Name = "Fecha inicio")]
        public string fechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de final es un campo requerido.")]
        [Display(Name = "Fecha final")]
        public string fechaFinal { get; set; }


        public List<string> modulos { get; set; }
         
    }
}