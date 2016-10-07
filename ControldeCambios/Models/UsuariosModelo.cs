using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace ControldeCambios.Models
{

    public class UsuariosModelo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Usuario usuario { get; set; }

        public ApplicationUser identityUsuario { get; set; }
        public IdentityRole rol { get; set; }

        [Required(ErrorMessage = "El campo Email es requerido.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras.")]
        public string nombreUsuario { get; set; }

        [StringLength(8), MinLength(8)]
        [Required(ErrorMessage = "El teléfono es un campo requerido."), RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de 8 dígitos")]
        [Display(Name = "Primer Telefono")]
        public string tel1 { get; set; }

        [StringLength(8), MinLength(8)]
        [Display(Name = "Segundo Telefono (opcional)")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de 8 dígitos")]
        public string tel2 { get; set; }

        [StringLength(8), MinLength(8)]
        [Display(Name = "Tercer Telefono (opcional)")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de 8 dígitos")]
        public string tel3 { get; set; }

        public List<Usuario> usuarios { get; set; }
        public List<ApplicationUser> identityUsuarios { get; set; }
        public List<IdentityRole> roles { get; set; }
        public List<Usuarios_Telefonos> telefonos { get; set; }

        public bool eliminarUsuario { get; set; }
        public bool modificarUsuario { get; set; }
        public bool crearUsuario { get; set; }
        public bool detallesUsuario { get; set; }
    }
}