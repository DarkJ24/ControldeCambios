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

        [Required(ErrorMessage = "El email es un campo requerido.")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido")]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Required(ErrorMessage = "El nombre es un campo requerido.")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "El nombre solo debe contener letras y espacios")]
        public string nombreUsuario { get; set; }

        [StringLength(8, ErrorMessage = "El teléfono debe ser de {1} dígitos"), MinLength(8, ErrorMessage = "El teléfono debe ser de almenos {1} dígitos")]
        [Required(ErrorMessage = "El teléfono es un campo requerido."), RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de dígitos")]
        [Display(Name = "Primer Telefono")]
        public string tel1 { get; set; }

        [StringLength(8, ErrorMessage = "El teléfono debe ser de {1} dígitos"), MinLength(8, ErrorMessage = "El teléfono debe ser de almenos {1} dígitos")]
        [Display(Name = "Segundo Telefono (opcional)")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de dígitos")]
        public string tel2 { get; set; }

        [StringLength(8, ErrorMessage = "El teléfono debe ser de {1} dígitos"), MinLength(8, ErrorMessage = "El teléfono debe ser de almenos {1} dígitos")]
        [Display(Name = "Tercer Telefono (opcional)")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono debe ser de dígitos")]
        public string tel3 { get; set; }

        public class userInfo
        {
            public string cedula;
            public string nombre;
            public string correo;
            public string rol;
            public string identityId;
        }

        public List<userInfo> indexUserInfoList;

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