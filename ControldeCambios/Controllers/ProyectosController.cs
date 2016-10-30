using ControldeCambios.App_Start;
using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            return View();
        }

        // GET: Detalles
        public ActionResult Detalles(string id)
        {
            ModificarProyectoModel model = new ModificarProyectoModel();

            var proyecto = baseDatos.Proyectos.Find(id);

            model.cliente = baseDatos.Usuarios.Find(proyecto.cliente);
            model.lider = baseDatos.Usuarios.Find(proyecto.lider);
            model.descripcion = proyecto.descripcion;
            model.fechaInicio = ((proyecto.fechaInicio) ?? default(DateTime)).ToString("MM/dd/yyyy");
            model.fechaFinal = (proyecto.fechaFinal ?? default(DateTime)).ToString("MM/dd/yyyy");
            model.nombre = proyecto.nombre;
            model.estado = proyecto.estado;
            var equipo = proyecto.Proyecto_Equipo.ToList();
            model.equipo = new List<string>();
            foreach (var des in equipo)
            {
                model.equipo.Add(des.usuario);
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

            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");

            return View(model);
        }

        // POST: Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(ModificarProyectoModel model)
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
                proyecto.estado = model.estado;

                proyecto.duracion = (proyecto.fechaFinal ?? default(DateTime)).Subtract((proyecto.fechaInicio ?? default(DateTime))).Days;
                baseDatos.Entry(proyecto).State = System.Data.Entity.EntityState.Modified;

                var equipo_viejo = baseDatos.Proyecto_Equipo.Where(m => m.proyecto == model.nombre).ToList();

                foreach(var usuario in equipo_viejo)
                {
                    baseDatos.Entry(usuario).State = System.Data.Entity.EntityState.Deleted;
                }

                if (model.equipo != null)
                {
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
                } else
                {
                    var proyectoDesarrollador = new Proyecto_Equipo();
                    proyectoDesarrollador.usuario = proyecto.lider;
                    proyectoDesarrollador.proyecto = proyecto.nombre;
                    baseDatos.Proyecto_Equipo.Add(proyectoDesarrollador);
                }




                baseDatos.SaveChanges();
                this.AddToastMessage("Proyecto modificado", "El proyecto " + model.nombre + " se ha modificado correctamente.", ToastType.Success);
                return RedirectToAction("Detalles", "Proyectos");
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

            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");

            return View(model);
        }
    }
}