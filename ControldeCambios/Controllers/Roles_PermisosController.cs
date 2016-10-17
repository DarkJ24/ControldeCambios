using System.Linq;
using System.Web.Mvc;
using ControldeCambios.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using ControldeCambios.App_Start;

namespace ControldeCambios.Controllers
{
    /// <summary>
    /// Provee funcionalidad administrar los permisos asociados a los roles
    /// </summary>
    public class Roles_PermisosController : ToastrController
    {
        ApplicationDbContext context;
        Entities baseDatos;


        /// <summary>
        /// Constructor del controlador de Roles_Permisos
        /// </summary>
        /// <returns>Pagina de Index</returns>
        public Roles_PermisosController()
        {
            context = new ApplicationDbContext();
            baseDatos = new Entities();
        }

        /// <summary>
        /// Se utiliza para revisar que el rol del usuario que intenta acceder a alguna
        /// caracteristica tenga los permisos correspondientes.
        /// </summary>
        /// <param name="permiso"> Nombre del permiso que se intenta revisar.</param>
        /// <returns>Pagina de Index</returns>
        private bool revisarPermisos(string permiso)
        {
            string userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var rol = context.Users.Find(userID).Roles.First();
            var permisoID = baseDatos.Permisos.Where(m => m.nombre == permiso).First().codigo;
            var listaRoles = baseDatos.Rol_Permisos.Where(m => m.permiso == permisoID).ToList().Select(n => n.rol);
            bool userRol = listaRoles.Contains(rol.RoleId);

            return userRol;
        }

        /// <summary>
        /// Despliega la pagina index.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        // GET: Roles_Permisos
        public ActionResult Index()
        {

            if(!revisarPermisos("Gestionar Permisos"))
            {
                this.AddToastMessage("Acceso Denegado", "No tienes el permiso para gestionar Roles!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

            Roles_Permisos modelo = new Roles_Permisos();
            modelo.roles = context.Roles.ToList();
            modelo.permisos = baseDatos.Permisos.ToList();
            modelo.rol_permisos = baseDatos.Rol_Permisos.ToList();
            modelo.rolPermisoId = new List<Roles_Permisos.Relacion_Rol_Permiso>();


            foreach (var rol in modelo.roles)
            {
                foreach(var permiso in modelo.permisos)
                {
                    Rol_Permisos rol_nuevo = new Rol_Permisos();
                    rol_nuevo.permiso = permiso.codigo;
                    rol_nuevo.rol = rol.Id;
                    bool exists = false;
                    foreach(var rol_permiso in modelo.rol_permisos)
                    {
                        if(rol_permiso.permiso == permiso.codigo && rol.Id == rol_permiso.rol)
                        {
                            exists = true;
                        } 
                    }
                    modelo.rolPermisoId.Add(new Roles_Permisos.Relacion_Rol_Permiso(rol.Id, permiso.codigo, exists));
                }
            }

            return View(modelo);
        }

        /// <summary>
        /// Despliega la pagina index.
        /// </summary>
        /// <param name="model"> Modelo con la informacion de los roles a desplegar </param>
        /// <returns>Pagina de Index</returns>
        //POST: Roles_Permisos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Roles_Permisos model)
        {
            var rol_permisos = baseDatos.Rol_Permisos.ToList();
            
            //Role y Permiso de Admin y Gestionar Permisos
            int permisoID = baseDatos.Permisos.Where(m => m.nombre == "Gestionar Permisos").First().codigo;
            string roleID = context.Roles.Where(m => m.Name == "Admin").First().Id;

            foreach (var rol_permiso in rol_permisos)
            {
                if ((rol_permiso.permiso != permisoID) || (rol_permiso.rol != roleID))
                {
                    baseDatos.Entry(rol_permiso).State = System.Data.Entity.EntityState.Deleted;
                }
            }
            baseDatos.SaveChanges();

            foreach (var relacion_rol_permiso in model.rolPermisoId)
            {
                if (relacion_rol_permiso.valor)
                {
                    if ((relacion_rol_permiso.rol != roleID) || (relacion_rol_permiso.permiso != permisoID))
                    {
                        var rolPermisosEntry = new Rol_Permisos();
                        rolPermisosEntry.permiso = relacion_rol_permiso.permiso;
                        rolPermisosEntry.rol = relacion_rol_permiso.rol;
                        baseDatos.Rol_Permisos.Add(rolPermisosEntry);
                    }
                }
                else
                {
                    if ((relacion_rol_permiso.rol == roleID) && (relacion_rol_permiso.permiso == permisoID))
                    {
                        this.AddToastMessage("Advertencia", "No se puede quitar gestionar permisos al administrador! Por lo tanto no se quitó.", ToastType.Warning);
                    }
                }
            }
            baseDatos.SaveChanges();

            this.AddToastMessage("Permisos Guardados", "Se han logrado asignar los permisos a sus respectivos roles correctamente.", ToastType.Success);
            return RedirectToAction("Index", "Roles_Permisos");
        }
    }
}