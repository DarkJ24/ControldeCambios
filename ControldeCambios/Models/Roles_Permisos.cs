using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControldeCambios.Models
{
    public class Roles_Permisos
    {
        public List<Rol_Permisos> rol_permisos { get; set; }
        public List<Permiso> permisos { get; set; }
        public List<IdentityRole> roles { get; set; }

        public Dictionary<System.Tuple<string, int>, bool> diccionario { get; set; } // la tupla es rol, permiso
    }
}