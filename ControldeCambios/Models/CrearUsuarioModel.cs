using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ControldeCambios.Models
{
    public class CrearUsuarioModel
    {
        [Display(Name = "UserRoles")]
        [Required(ErrorMessage = "El campo Rol es requerido.")]
        public string UserRoles { get; set; }

        [Required(ErrorMessage = "El campo Email es requerido.")]
        [EmailAddress(ErrorMessage = "No es un Email válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo Password es requerido.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El campo Confirm Password es requerido.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La cédula es un campo requerido.")]
        [StringLength(9), MinLength(9)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "La cédula debe ser de 9 dígitos")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [StringLength(8), MinLength(8)]
        [Required(ErrorMessage = "El teléfono es un campo requerido."), RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de 8 dígitos")]
        [Display(Name = "Primer Telefono")]
        public string Telefono { get; set; }

        [StringLength(8), MinLength(8)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de 8 dígitos")]
        [Display(Name = "Segundo Telefono (opcional)")]
        public string Telefono2 { get; set; }

        [StringLength(8), MinLength(8)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de 8 dígitos")]
        [Display(Name = "Tercer Telefono (opcional)")]
        public string Telefono3 { get; set; }
    }
}