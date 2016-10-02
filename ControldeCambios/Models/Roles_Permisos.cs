using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControldeCambios.Models
{
    public class Roles_Permisos
    {
        public List<Rol_Permisos> rol_permisos = new List<Rol_Permisos>();
        public List<Permiso> permisos = new List<Permiso>();
        public List<IdentityRole> roles = new List<IdentityRole>();
    }
}