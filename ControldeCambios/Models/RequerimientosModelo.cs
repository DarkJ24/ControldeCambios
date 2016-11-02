using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Models
{
    public class RequerimientosModelo
    {
        //Tabla Requerimientos

        public Requerimiento requerimiento { get; set; }


        [Required(ErrorMessage = "El código es un campo requerido.")]
        [StringLength(15, ErrorMessage = "El código debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El código debe ser de al menos {1} dígito")]
        [Display(Name = "Código")]
        [RegularExpression(@"^[A-Za-z0-9\-\s]+$", ErrorMessage = "El código solo debe contener letras, números, espacios y guiones")]
        public string codigo { get; set; }


        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [StringLength(25, ErrorMessage = "El nombre debe ser de {1} caracteres"), MinLength(1, ErrorMessage = "El nombre debe ser de al menos {1} caracteres")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        public string nombre { get; set; }

        public string version { get; set; }

        [StringLength(120, ErrorMessage = "La descripción debe ser de {1} caracteres máximo"), MinLength(1, ErrorMessage = "La descripción debe ser de al menos {1} caracter")]
        [Display(Name = "Descripción")]
        public string descripcion { get; set; }

        
        public string solicitadoPor { get; set; }

        public string creadoPor { get; set; }

        public string modulo { get; set; }


        [Required(ErrorMessage = "La prioridad es un campo requerido.")]
        [Display(Name = "Prioridad")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "La prioridad solo puede contener números")]
        public string prioridad { get; set; }


        [Display(Name = "Esfuerzo")]
        [RegularExpression(@"^[0-9\s]+$", ErrorMessage = "El esfuerzo solo pueden contener números y espacios")]
        public string esfuerzo { get; set; }


        [Display(Name = "Fecha de Inicio")]
        public string fechaInicial { get; set; }


        [Display(Name = "Fecha Final")]
        public string fechaFinal { get; set; }


        public string estado { get; set; }


        [Display(Name = "Observaciones")]
        [RegularExpression(@"^[A-Za-z0-9\s]+$", ErrorMessage = "Las observaciones solo pueden contener números, letras y espacios")]
        public string observaciones { get; set; }


        //AGREGAR IMAGEN

        [Display(Name = "Criterios de Aceptación")]
        [Required]
        public string criteriosAceptacion { get; set; }


        //AGREGAR EQUIPO
        public List<string> equipo { get; set; }

        public string proyecto { get; set; }

        public int id { get; set; }

        public bool eliminarRequerimiento { get; set; }
        public bool modificarRequerimiento { get; set; }
        public bool crearRequerimiento { get; set; }
        public bool detallesRequerimiento { get; set; }

    }
}
