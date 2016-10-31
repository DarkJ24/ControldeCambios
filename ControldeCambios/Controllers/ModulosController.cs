using ControldeCambios.App_Start;
using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Controllers
{
    public class ModulosController : ToastrController
    {
        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

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

        // GET: Modulos
        public ActionResult Index()
        {
            return View();
        }

        // GET: Modulos/Crear
        public ActionResult Crear(string proyecto)
        {
            if (!revisarPermisos("Modificar Proyectos"))
            {
                //Despliega mensaje en caso de no poder crear un proyecto
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear módulos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (String.IsNullOrEmpty(proyecto))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (baseDatos.Proyectos.Find(proyecto) != null)
            {
                var model = new ModulosModel();
                model.proyecto = proyecto;
                return View(model);
            } else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Funcionalidad para crear Módulos.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Módulo a crear.</param>
        /// <returns>Pagina de Index</returns>
        // POST: /Modulos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(ModulosModel model)
        {
            if (ModelState.IsValid)
            {
                var modulo = new Modulo();
                modulo.proyecto = model.proyecto;
                modulo.numero = Int32.Parse(model.numero);
                if (baseDatos.Modulos.Where(m => m.numero == modulo.numero && m.proyecto == modulo.proyecto).Count() > 0)
                {
                    this.AddToastMessage("Módulo Ya Existe", "Ya existe un módulo con ese número para ese proyecto.", ToastType.Error);
                    return View(model);
                } else
                {
                    modulo.nombre = model.nombre;
                    baseDatos.Modulos.Add(modulo);
                    baseDatos.SaveChanges();
                    this.AddToastMessage("Módulo Creado", "El módulo " + model.nombre + " se ha creado correctamente.", ToastType.Success);
                    return RedirectToAction("Crear", "Modulos", new { proyecto = model.proyecto});
                }
            }
            return View(model);
        }

        // GET: Modulos/Detalles
        public ActionResult Detalles(string proyecto, string numero)
        {
            if (!revisarPermisos("Modificar Proyectos"))
            {
                //Despliega mensaje en caso de no poder crear un proyecto
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para modificar módulos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (String.IsNullOrEmpty(proyecto) || String.IsNullOrEmpty(numero))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var modulo = new Modulo();
            modulo.proyecto = proyecto;
            modulo.numero = Int32.Parse(numero);
            var listaDeModulos = baseDatos.Modulos.Where(m => m.numero == modulo.numero && m.proyecto == modulo.proyecto);
            if (listaDeModulos.Count() > 0)
            {
                var modulo1 = listaDeModulos.First();
                var model = new ModulosModel();
                model.numero = numero;
                model.nombre = modulo1.nombre;
                model.proyecto = proyecto;
                return View(model);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Funcionalidad para modificar Módulos.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Módulo a modificar.</param>
        /// <returns>Pagina de Index</returns>
        // POST: /Modulos/Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(ModulosModel model)
        {
            if (ModelState.IsValid)
            {
                var modulo = new Modulo();
                modulo.proyecto = model.proyecto;
                modulo.numero = Int32.Parse(model.numero);
                modulo.nombre = model.nombre;
                baseDatos.Entry(modulo).State = System.Data.Entity.EntityState.Modified;
                baseDatos.SaveChanges();
                this.AddToastMessage("Módulo Modificado", "El módulo " + model.nombre + " se ha modificado correctamente.", ToastType.Success);
                return RedirectToAction("Detalles", "Modulos", new { proyecto = model.proyecto, numero = model.numero });
            }
            return View(model);
        }
    }
}