using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ControldeCambios.Models
{
    public class AprobarSolicitudCambioModelo
    {
        [Required(ErrorMessage = "El código es un campo requerido.")]
        [StringLength(15, ErrorMessage = "El código debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El código debe ser de al menos {1} dígito")]
        [Display(Name = "Código")]
        [RegularExpression(@"^[A-Za-z0-9\-\s]+$", ErrorMessage = "El código solo debe contener letras, números, espacios y guiones")]
        public string codigo1 { get; set; }

        [Required(ErrorMessage = "El código es un campo requerido.")]
        [StringLength(15, ErrorMessage = "El código debe ser de {1} dígitos"), MinLength(1, ErrorMessage = "El código debe ser de al menos {1} dígito")]
        [Display(Name = "Código")]
        [RegularExpression(@"^[A-Za-z0-9\-\s]+$", ErrorMessage = "El código solo debe contener letras, números, espacios y guiones")]
        public string codigo2 { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [StringLength(25, ErrorMessage = "El nombre debe ser de {1} caracteres"), MinLength(1, ErrorMessage = "El nombre debe ser de al menos {1} caracteres")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        public string nombre1 { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [StringLength(25, ErrorMessage = "El nombre debe ser de {1} caracteres"), MinLength(1, ErrorMessage = "El nombre debe ser de al menos {1} caracteres")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        public string nombre2 { get; set; }

        [StringLength(120, ErrorMessage = "La descripción debe ser de {1} caracteres máximo"), MinLength(1, ErrorMessage = "La descripción debe ser de al menos {1} caracter")]
        [Display(Name = "Descripción")]
        public string descripcion1 { get; set; }

        [StringLength(120, ErrorMessage = "La descripción debe ser de {1} caracteres máximo"), MinLength(1, ErrorMessage = "La descripción debe ser de al menos {1} caracter")]
        [Display(Name = "Descripción")]
        public string descripcion2 { get; set; }

        [Required(ErrorMessage = "El nombre del solicitante es un campo requerido.")]
        public string solicitadoPor1 { get; set; }

        [Required(ErrorMessage = "El nombre del solicitante es un campo requerido.")]
        public string solicitadoPor2 { get; set; }

        [Required(ErrorMessage = "El nombre del creador es un campo requerido.")]
        public string creadoPor1 { get; set; }

        [Required(ErrorMessage = "El nombre del creador es un campo requerido.")]
        public string creadoPor2 { get; set; }

        [Required(ErrorMessage = "La prioridad es un campo requerido.")]
        [Display(Name = "Prioridad")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "La prioridad solo puede contener números")]
        public string prioridad1 { get; set; }

        [Required(ErrorMessage = "La prioridad es un campo requerido.")]
        [Display(Name = "Prioridad")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "La prioridad solo puede contener números")]
        public string prioridad2 { get; set; }

        [Display(Name = "Esfuerzo")]
        [RegularExpression(@"^[0-9\s]+$", ErrorMessage = "El esfuerzo solo pueden contener números y espacios")]
        [Required(ErrorMessage = "El esfuerzo es un campo requerido.")]
        public string esfuerzo1 { get; set; }

        [Display(Name = "Esfuerzo")]
        [Required(ErrorMessage = "El esfuerzo es un campo requerido.")]
        [RegularExpression(@"^[0-9\s]+$", ErrorMessage = "El esfuerzo solo pueden contener números y espacios")]
        public string esfuerzo2 { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [Required(ErrorMessage = "La fecha de inicio es un campo requerido.")]
        public string fechaInicial1 { get; set; }

        [Display(Name = "Fecha de Inicio")]
        public string fechaInicial2 { get; set; }

        [Display(Name = "Fecha Final")]
        public string fechaFinal1 { get; set; }

        [Display(Name = "Fecha Final")]
        public string fechaFinal2 { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es un campo requerido.")]
        public string estado1 { get; set; }

        [Display(Name = "Estado")]
        [Required(ErrorMessage = "El estado es un campo requerido.")]
        public string estado2 { get; set; }

        [Display(Name = "Observaciones")]
        [RegularExpression(@"^[A-Za-z0-9\s]+$", ErrorMessage = "Las observaciones solo pueden contener números, letras y espacios")]
        public string observaciones1 { get; set; }

        [Display(Name = "Observaciones")]
        [RegularExpression(@"^[A-Za-z0-9\s]+$", ErrorMessage = "Las observaciones solo pueden contener números, letras y espacios")]
        public string observaciones2 { get; set; }

        ///Imagen///
        public string file1 { get; set; }
        public string file2 { get; set; }

        [Display(Name = "Criterios de Aceptación")]
        [Required(ErrorMessage = "Los criterios de aceptación son un campo requerido.")]
        public string criteriosAceptacion1 { get; set; }

        [Display(Name = "Criterios de Aceptación")]
        [Required(ErrorMessage = "Los criterios de aceptación son un campo requerido.")]
        public string criteriosAceptacion2 { get; set; }

        //AGREGAR EQUIPO
        public List<string> equipo1 { get; set; }
        public List<string> equipo2 { get; set; }

        [Display(Name = "Proyecto")]
        [Required(ErrorMessage = "El proyecto es un campo requerido.")]
        public string proyecto { get; set; }

        public int id1 { get; set; }
        public int id2 { get; set; }

        public string version1 { get; set; }
        public string version2 { get; set; }
        
        //DATOS DE SOLICITUD
        [Required(ErrorMessage = "El comentario es un campo obligatorio.")]
        [Display(Name = "Comentario")]
        public string comentario { get; set; }

        [Required(ErrorMessage = "La fecha de la solicitud es un campo obligatorio.")]
        [Display(Name = "Fecha de Solicitud")]
        public string solicitadoEn { get; set; }

        [Required(ErrorMessage = "El estado de la solicitud es un campo obligatorio.")]
        [Display(Name = "Estado de Solicitud")]
        public string estado { get; set; }

        [Required(ErrorMessage = "El tipo de solicitud es un campo obligatorio.")]
        [Display(Name = "Tipo de Solicitud")]
        public string tipo { get; set; }

        [Required(ErrorMessage = "El solicitante es un campo obligatorio.")]
        [Display(Name = "Solicitante")]
        public string solicitadoPor { get; set; }

        [Required(ErrorMessage = "La razón es un campo obligatorio.")]
        [Display(Name = "Razón")]
        public string razon { get; set; }
    }
}