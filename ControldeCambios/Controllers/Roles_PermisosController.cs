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
            modelo.diccionario = new Dictionary<System.Tuple<string, int>, bool>();

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


                    modelo.diccionario[System.Tuple.Create(rol.Id, permiso.codigo)] = exists;
                }
            }

            return View(modelo);
        }
    }
}