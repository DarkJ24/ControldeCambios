using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControldeCambios.Models
{
    public class UsuariosModelo
    {
        public Usuario usuario = new Usuario();
        public ApplicationUser identityUsuario = new ApplicationUser();
        public IdentityRole rol = new IdentityRole();
        public Usuarios_Telefonos tel1 = new Usuarios_Telefonos();
        public Usuarios_Telefonos tel2 = new Usuarios_Telefonos();
        public List<Usuario> usuarios = new List<Usuario>();
        public List<ApplicationUser> identityUsuarios = new List<ApplicationUser>();
        public List<IdentityRole> roles = new List<IdentityRole>();
        public List<Usuarios_Telefonos> telefonos = new List<Usuarios_Telefonos>();
    }
}