using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControldeCambios.Models
{
    /// <summary>
    /// Provee modelado para los objetos de tipo Rol_Permiso
    /// </summary>
    public class Roles_Permisos
    {
        public class Relacion_Rol_Permiso
        {
            public Relacion_Rol_Permiso()
            { 
            }

            public Relacion_Rol_Permiso(string id, int codigo, bool exists)
            {
                this.rol = id;
                this.permiso = codigo;
                this.valor = exists;
            }

            public string rol { get; set; }
            public int permiso { get; set; }
            public bool valor { get; set; }
        }

        public List<Rol_Permisos> rol_permisos { get; set; }
        public List<Permiso> permisos { get; set; }
        public List<IdentityRole> roles { get; set; }

        public List<Relacion_Rol_Permiso> rolPermisoId { get; set; }


    }
}