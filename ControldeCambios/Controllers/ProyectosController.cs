using ControldeCambios.App_Start;
using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Controllers
{
    public class ProyectosController : ToastrController
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

        // GET: Proyectos
        public ActionResult Index()
        {
            return View();
        }
        // GET: Crear
        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Proyectos"))
            {
                //despliega mensaje en caso de no poder crear un usuario
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear proyectos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Desarrolladores = new SelectList(baseDatos.Usuarios.ToList(), "cedula", "nombre");
            ViewBag.Clientes = new SelectList(baseDatos.Usuarios.ToList(), "cedula", "nombre");
            ViewBag.Usuarios = baseDatos.Usuarios.ToList();
            return View();
        }
    }
}