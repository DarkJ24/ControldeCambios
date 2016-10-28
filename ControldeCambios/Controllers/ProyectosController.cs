using ControldeCambios.App_Start;
using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        public ActionResult Index(int? page)
        {
            if (!revisarPermisos("Consultar Lista de Proyectos"))
            {
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar Usuarios!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

            ProyectosModelo modelo = new ProyectosModelo();

            /*modelo.nombre = context;
            modelo.proyecto;
            modelo.cliente;
            modelo.descripcion;
            modelo.equipo;
            modelo.fechaFinal;
            modelo.fechaInicio;
            modelo.lider;*/
            

            return View();
        }
        // GET: Crear
        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Proyectos"))
            {
                //despliega mensaje en caso de no poder crear un proyecto
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear proyectos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            string desarrolladorRol = context.Roles.Where(m => m.Name == "Desarrollador").First().Id;
            foreach (var user in context.Users.ToArray())
            {
                if (user.Roles.First().RoleId.Equals(clienteRol))
                {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                } else
                {
                    if (user.Roles.First().RoleId.Equals(desarrolladorRol))
                    {
                        listaDesarrolladores.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                    }
                }
            }
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            return View();
        }

        /// <summary>
        /// Funcionalidad para crear Proyectos.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Proyecto a crear.</param>
        /// <returns>Pagina de Index</returns>
        // POST: /Proyectos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(ProyectosModelo model)
        {

            if (ModelState.IsValid)
            {
                var proyecto = new Proyecto();
                proyecto.cliente = model.cliente.cedula;
                proyecto.lider = model.lider.cedula;
                proyecto.fechaInicio = DateTime.ParseExact(model.fechaInicio, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                proyecto.fechaFinal = DateTime.ParseExact(model.fechaFinal, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                proyecto.descripcion = model.descripcion;
                proyecto.nombre = model.nombre;
                proyecto.estado = "Por iniciar";
                proyecto.duracion = proyecto.fechaFinal.Subtract(proyecto.fechaInicio).Days;
                baseDatos.Proyectos.Add(proyecto);
                foreach (var desarrollador in model.equipo)
                {
                    var proyectoDesarrollador = new Proyecto_Equipo();
                    proyectoDesarrollador.usuario = desarrollador;
                    proyectoDesarrollador.proyecto = proyecto.nombre;
                    baseDatos.Proyecto_Equipo.Add(proyectoDesarrollador);
                }
                var checkLider = model.equipo.Where(m => m == proyecto.lider);
                if (checkLider.Count() == 0)
                {
                    var proyectoDesarrollador = new Proyecto_Equipo();
                    proyectoDesarrollador.usuario = proyecto.lider;
                    proyectoDesarrollador.proyecto = proyecto.nombre;
                    baseDatos.Proyecto_Equipo.Add(proyectoDesarrollador);
                }
                baseDatos.SaveChanges();
                this.AddToastMessage("Proyecto Creado", "El proyecto " + model.nombre + " se ha creado correctamente.", ToastType.Success);
                return RedirectToAction("Crear", "Proyectos");
            }
            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            string desarrolladorRol = context.Roles.Where(m => m.Name == "Desarrollador").First().Id;
            foreach (var user in context.Users.ToArray())
            {
                if (user.Roles.First().RoleId.Equals(clienteRol))
                {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                }
                else
                {
                    if (user.Roles.First().RoleId.Equals(desarrolladorRol))
                    {
                        listaDesarrolladores.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                    }
                }
            }
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            return View(model);
        }
    }
}