using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ControldeCambios.Models
{
    /// <summary>
    /// Provee modelado para los objetos de tipo Usuario
    /// </summary>
    public class CrearUsuarioModel
    {
        [Display(Name = "UserRoles")]
        [Required(ErrorMessage = "El rol es un campo requerido.")]
        public string UserRoles { get; set; }

        [Required(ErrorMessage = "El email campo requerido.")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La cédula es un campo requerido.")]
        [StringLength(9, ErrorMessage = "La cédula debe ser de {1} dígitos"), MinLength(9, ErrorMessage = "La cédula debe ser de almenos {1} dígitos")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "La cédula debe ser de dígitos")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [StringLength(8, ErrorMessage = "El teléfono debe ser de {1} dígitos"), MinLength(8, ErrorMessage = "El teléfono debe ser de almenos {1} dígitos")]
        [Required(ErrorMessage = "El teléfono es un campo requerido."), RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de dígitos")]
        [Display(Name = "Primer Telefono")]
        public string Telefono { get; set; }

        [StringLength(8, ErrorMessage = "El teléfono debe ser de {1} dígitos"), MinLength(8, ErrorMessage = "El teléfono debe ser de almenos {1} dígitos")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de dígitos")]
        [Display(Name = "Segundo Telefono (opcional)")]
        public string Telefono2 { get; set; }

        [StringLength(8, ErrorMessage = "El teléfono debe ser de {1} dígitos"), MinLength(8, ErrorMessage = "El teléfono debe ser de almenos {1} dígitos")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de dígitos")]
        [Display(Name = "Tercer Telefono (opcional)")]
        public string Telefono3 { get; set; }
    }
}