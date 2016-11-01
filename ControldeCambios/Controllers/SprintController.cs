using ControldeCambios.App_Start;
using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Controllers
{
    public class SprintController : ToastrController
    {

        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

        // GET: Sprint
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Proyectos"))
            {
                //despliega mensaje en caso de no poder crear un usuario
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear sprints!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Proyectos = new SelectList(baseDatos.Proyectos.ToList(), "nombre", "nombre");
            ViewBag.Requerimientos = new MultiSelectList(baseDatos.Requerimientos.ToList(), "id", "nombre");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(SprintModelo model)
        {
            if (ModelState.IsValid)
            {
                var sprint = new Sprint();
                
                sprint.numero = model.numero;
                sprint.proyecto = model.proyecto;

                sprint.fechaInicio = DateTime.ParseExact(model.fechaInicio, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                sprint.fechaFinal = DateTime.ParseExact(model.fechaFinal, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                sprint.Requerimientos = model.requerimientos.Select(m => baseDatos.Requerimientos.Find(Int32.Parse(m))).ToList();

                baseDatos.Sprints.Add(sprint);
                baseDatos.SaveChanges();


                this.AddToastMessage("Sprint Creado", "El sprint " + model.numero + " se ha creado y asignado correctamente"
                    + " al proyecto " + model.proyecto + ".", ToastType.Success);

                
                return RedirectToAction("Crear", "Sprint");
                
            }
            
                ViewBag.Proyectos = new SelectList(baseDatos.Proyectos.ToList(), "nombre", "nombre");
                ViewBag.Requerimientos = baseDatos.Requerimientos.ToList();
                return View(model);
          
            //return View(model);
        }
        /// <summary>
        /// Se utiliza para revisar que el rol del usuario que intenta acceder a alguna
        /// caracteristica tenga los permisos correspondientes.
        /// </summary>
        /// <param name="permiso"> Nombre del permiso que se intenta revisar.</param>
        /// <returns>Pagina de Index</returns>
        private bool revisarPermisos(string permiso)
        {
            String userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var rol = context.Users.Find(userID).Roles.First();
            var permisoID = baseDatos.Permisos.Where(m => m.nombre == permiso).First().codigo;
            var listaRoles = baseDatos.Rol_Permisos.Where(m => m.permiso == permisoID).ToList().Select(n => n.rol);
            bool userRol = listaRoles.Contains(rol.RoleId);

            return userRol;
        }
    }
}