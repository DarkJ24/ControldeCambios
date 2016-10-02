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
        public string UserRoles { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Cedula")]
        public string Cedula { get; set; }

        [Required]
        [Display(Name = "Primer Telefono")]
        public string Telefono { get; set; }

        [Display(Name = "Segundo Telefono (opcional)")]
        public string Telefono2 { get; set; }

        [Display(Name = "Tercer Telefono (opcional)")]
        public string Telefono3 { get; set; }
    }
}