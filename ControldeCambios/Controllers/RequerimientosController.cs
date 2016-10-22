using System.Linq;
using System.Web.Mvc;
using ControldeCambios.Models;
using System.Net;
using System;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using ControldeCambios.App_Start;
using System.Web.Security;
using PagedList;
using System.Collections.Generic;

namespace ControldeCambios.Controllers
{
    public class RequerimientosController : ToastrController
    {

        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        // GET: Requerimientos
        public ActionResult Index(string id)
        {
            var proyecto = baseDatos.Proyectos.Find(id);
            var model = new RequerimientosIndexModel();

            model.titulo = proyecto.nombre;
            model.descripcion = proyecto.descripcion;
            model.cliente = proyecto.cliente;
            model.lider = proyecto.lider;

            var sprints = baseDatos.Sprints.Where(m => m.proyecto == id).ToList();

            var numeros_sprints = sprints.Select(m => m.numero);
            var sprint_modulos = baseDatos.Sprint_Modulo.Where(m => numeros_sprints.Contains(m.sprint) && m.proyecto == id).ToList();
            var sprint_modulo_requerimientos = sprint_modulos.Select(m => m.Requerimientos.ToList());

            if (sprint_modulo_requerimientos.Any())
            {
                model.reqs = sprint_modulo_requerimientos.Aggregate((acc, x) => acc.Concat(x).ToList());
            }
            else
            {
                model.reqs = new List<Requerimiento>();
            }

            return View(model);
        }


        private bool revisarPermisos(string permiso)
        {
            String userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var rol = context.Users.Find(userID).Roles.First();
            var permisoID = baseDatos.Permisos.Where(m => m.nombre == permiso).First().codigo;
            var listaRoles = baseDatos.Rol_Permisos.Where(m => m.permiso == permisoID).ToList().Select(n => n.rol);
            bool userRol = listaRoles.Contains(rol.RoleId);

            return userRol;
        }


        /// <summary>
        /// Funcionalidad para crear Requerimientos.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        // GET: /Requerimientos/Crear
        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Requerimientos"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Usuarios = baseDatos.Usuarios.ToList();
            return View();
        }


        /// <summary>
        /// Funcionalidad para crear Requerimientos.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Requerimiento a crear.</param>
        /// <returns>Pagina de Index</returns>
        // POST: /Requerimientos/Crear
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(RequerimientosModelo model)
        {
            if (ModelState.IsValid)
            {
                var requerimiento = new Requerimiento();

                requerimiento.codigo = model.codigo;
                requerimiento.nombre = model.nombre;
                requerimiento.descripcion = model.descripcion;
                requerimiento.solicitadoPor = model.solicitadoPor.ToString();


                baseDatos.Requerimientos.Add(requerimiento);
                baseDatos.SaveChanges();
            }

            return View(model);
        }*/
    }
}