using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Models
{
    /// <summary>
    /// Provee modelado para los objetos de tipo Requerimiento
    /// </summary>
    
    public class CrearSolicitudModel
    {

        [Required(ErrorMessage = "El código es un campo requerido.")]
        [StringLength(15, ErrorMessage = "El código debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El código debe ser de al menos {1} dígito")]
        [Display(Name = "Código")]
        [RegularExpression(@"^[A-Za-z0-9\-\s]+$", ErrorMessage = "El código solo debe contener letras, números, espacios y guiones.")]
        public string codigo { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [StringLength(25, ErrorMessage = "El nombre debe ser de {1} caracteres"), MinLength(1, ErrorMessage = "El nombre debe ser de al menos {1} caracteres")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios.")]
        public string nombre { get; set; }

        [StringLength(120, ErrorMessage = "La descripción debe ser de {1} caracteres máximo"), MinLength(1, ErrorMessage = "La descripción debe ser de al menos {1} caracter")]
        [Display(Name = "Descripción")]
        public string descripcion { get; set; }
        
        [Required(ErrorMessage = "El solicitante es un campo requerido.")]
        [Display(Name = "Solicitado Por")]
        public string solicitadoPor { get; set; }

        [Required(ErrorMessage = "El creador es un campo requerido.")]
        [Display(Name = "Creado Por")]
        public string creadoPor { get; set; }

        [Required(ErrorMessage = "La prioridad es un campo requerido.")]
        [Display(Name = "Prioridad")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "La prioridad solo puede contener números")]
        public string prioridad { get; set; }

        [Display(Name = "Esfuerzo")]
        [Required(ErrorMessage = "El esfuerzo es un campo requerido.")]
        [RegularExpression(@"^[0-9\s]+$", ErrorMessage = "El esfuerzo solo pueden contener números y espacios")]
        public string esfuerzo { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [Required(ErrorMessage = "La fecha de inicio es un campo requerido.")]
        public string fechaInicial { get; set; }

        [Display(Name = "Fecha Final")]
        public string fechaFinal { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es un campo requerido.")]
        public string estado { get; set; }

        [Display(Name = "Observaciones")]
        [RegularExpression(@"^[A-Za-z0-9\s]+$", ErrorMessage = "Las observaciones solo pueden contener números, letras y espacios")]
        public string observaciones { get; set; }

        public string file { get; set; }

        [Display(Name = "Criterios de Aceptación")]
        [Required(ErrorMessage = "Los criterios de aceptación son un campo requerido.")]
        public string criteriosAceptacion { get; set; }

        public List<string> equipo { get; set; }

        [Display(Name = "Proyecto")]
        [Required(ErrorMessage = "El proyecto es un campo requerido.")]
        public string proyecto { get; set; }

        [Required(ErrorMessage = "El id de requerimiento anterior es un campo requerido.")]
        public int idReqAnterior { get; set; }

        //Razones de Cambio
        [StringLength(200, ErrorMessage = "La razón debe ser de {1} caracteres máximo"), MinLength(1, ErrorMessage = "La razón debe ser de al menos {1} caracter")]
        [Display(Name = "Razón")]
        [Required(ErrorMessage = "El motivo es un campo requerido.")]
        public string razon { get; set; }

        [StringLength(200, ErrorMessage = "La razón debe ser de {1} caracteres máximo"), MinLength(1, ErrorMessage = "La razón debe ser de al menos {1} caracter")]
        public string razon2 { get; set; }

        public string version { get; set; }

        public bool crearSolicitud { get; set; }
        public bool eliminarRequerimiento { get; set; }

        //Index

        public List<Requerimiento> reqs { get; set; }
    }
}
