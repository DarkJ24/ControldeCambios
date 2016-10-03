using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControldeCambios.Models
{
    public class UsuariosModelo
    {
        public Usuario usuario { get; set; }
        public ApplicationUser identityUsuario { get; set; }
        public IdentityRole rol { get; set; }
        public Usuarios_Telefonos tel1 { get; set; }
        public Usuarios_Telefonos tel2 { get; set; }
        public List<Usuario> usuarios { get; set; }
        public List<ApplicationUser> identityUsuarios { get; set; }
        public List<IdentityRole> roles { get; set; }
        public List<Usuarios_Telefonos> telefonos { get; set; }
    }
}