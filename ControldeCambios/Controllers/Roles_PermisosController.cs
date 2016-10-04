using System.Linq;
using System.Web.Mvc;
using ControldeCambios.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ControldeCambios.Controllers
{

    public class Roles_PermisosController : Controller
    {
        ApplicationDbContext context;
        Entities baseDatos;

        public Roles_PermisosController()
        {
            context = new ApplicationDbContext();
            baseDatos = new Entities();
        }

        // GET: Roles_Permisos
        public ActionResult Index()
        {
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

        //POST: Roles_Permisos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Roles_Permisos model)
        {
            var rol_permisos = baseDatos.Rol_Permisos.ToList();

            foreach (var rol_permiso in rol_permisos)
            {
 
                    baseDatos.Entry(rol_permiso).State = System.Data.Entity.EntityState.Deleted;
            }
            baseDatos.SaveChanges();
       

            foreach(var relacion_rol_permiso in model.rolPermisoId)
            {
                if(relacion_rol_permiso.valor)
                {
                    var rolPermisosEntry = new Rol_Permisos();
                    rolPermisosEntry.permiso = relacion_rol_permiso.permiso;
                    rolPermisosEntry.rol = relacion_rol_permiso.rol;
                    baseDatos.Rol_Permisos.Add(rolPermisosEntry);
                }
            }
            baseDatos.SaveChanges();

            return RedirectToAction("Index", "Roles_Permisos");
        }
    }
}