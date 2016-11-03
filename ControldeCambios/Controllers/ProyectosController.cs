using ControldeCambios.App_Start;
using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
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
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar Proyectos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

            ProyectosModelo modelo = new ProyectosModelo();
            //Si es desarrollador, falta revisar pertenencia a proyectos
            // (solo debe mostrar proyectos a los que pertenezco)
            //Si es cliente, solo debe mostrar sus proyectos solicitados
            //Si es admin, mostrar todos
            modelo.proyectos = baseDatos.Proyectos.OrderByDescending(s => s.nombre).ToList();
            modelo.indexProyectoInfoList = new List<ProyectosModelo.proyectoInfo>();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (modelo.proyectos.Count < pageSize * pageNumber) ? modelo.proyectos.Count : pageSize * pageNumber;

            //despliega la informacion de los usuarios por paginas
            for (int i = (pageNumber - 1) * pageSize; i < lastElement; i++)
            {

                ProyectosModelo.proyectoInfo proyecto = new ProyectosModelo.proyectoInfo();
                proyecto.nombre = modelo.proyectos.ElementAt(i).nombre;
                proyecto.lider = baseDatos.Usuarios.Find(modelo.proyectos.ElementAt(i).lider).nombre;
                proyecto.cliente = baseDatos.Usuarios.Find(modelo.proyectos.ElementAt(i).cliente).nombre;
                proyecto.estado = modelo.proyectos.ElementAt(i).estado;

                modelo.indexProyectoInfoList.Add(proyecto);
            }
            modelo.crearProyecto = revisarPermisos("Crear Proyectos");
            modelo.detallesProyecto = revisarPermisos("Consultar Detalles de Proyectos");
            var proyectosAsIPagedList = new StaticPagedList<ProyectosModelo.proyectoInfo>(modelo.indexProyectoInfoList, pageNumber, pageSize, modelo.proyectos.Count);
            ViewBag.OnePageOfProyectos = proyectosAsIPagedList;
            return View(modelo);
        }

        // GET: Crear
        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Proyectos"))
            {
                //Despliega mensaje en caso de no poder crear un proyecto
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

        // GET: Detalles
        public ActionResult Detalles(string id)
        {
            if (!revisarPermisos("Modificar Proyectos"))
            {
                //Despliega mensaje en caso de no poder modificar un proyecto
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para modificar proyectos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ModificarProyectoModel model = new ModificarProyectoModel();
            var proyecto = baseDatos.Proyectos.Find(id);
            model.cliente = baseDatos.Usuarios.Find(proyecto.cliente);
            model.lider = baseDatos.Usuarios.Find(proyecto.lider);
            model.descripcion = proyecto.descripcion;
            model.fechaInicio = (proyecto.fechaInicio).ToString("MM/dd/yyyy");
            model.fechaFinal = (proyecto.fechaFinal).ToString("MM/dd/yyyy");
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
            model.eliminarPermitido = (revisarPermisos("Eliminar Proyectos") && (model.estado == "Por iniciar") && (baseDatos.Requerimientos.Where(m => m.proyecto == id).Count() == 0) && (baseDatos.Sprints.Where(m => m.proyecto == id).Count() == 0) && (baseDatos.Modulos.Where(m => m.proyecto == id).Count() == 0));
            model.modificarProyecto = revisarPermisos("Modificar Proyectos");
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
                proyecto.duracion = (proyecto.fechaFinal).Subtract(proyecto.fechaInicio).Days;
                baseDatos.Entry(proyecto).State = System.Data.Entity.EntityState.Modified;

                var equipo_viejo = baseDatos.Proyecto_Equipo.Where(m => m.proyecto == model.nombre).ToList();

                foreach (var usuario in equipo_viejo)
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
                }
                else
                {
                    var proyectoDesarrollador = new Proyecto_Equipo();
                    proyectoDesarrollador.usuario = proyecto.lider;
                    proyectoDesarrollador.proyecto = proyecto.nombre;
                    baseDatos.Proyecto_Equipo.Add(proyectoDesarrollador);
                }

                baseDatos.SaveChanges();
                this.AddToastMessage("Proyecto Modificado", "El proyecto " + model.nombre + " se ha modificado correctamente.", ToastType.Success);
                return RedirectToAction("Detalles", "Proyectos", new { id = proyecto.nombre });

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

        // GET: Informacion
        public ActionResult Informacion(string id, int ? sprintPage, int? moduloPage)
        {
            if (!revisarPermisos("Consultar Detalles de Proyectos"))
            {
                //Despliega mensaje en caso de no poder modificar un proyecto
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para ver detalles de proyectos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new ProyectoInfoModel();
            model.proyecto = baseDatos.Proyectos.Find(id);
            model.sprints = baseDatos.Sprints.Where(m => m.proyecto == model.proyecto.nombre).ToList();
            model.modulos = baseDatos.Modulos.Where(m => m.proyecto == model.proyecto.nombre).ToList();
            model.indexSprintInfoList = new List<ProyectoInfoModel.sprintInfo>();
            int pageSize = 10;
            int pageNumber = (sprintPage ?? 1);
            int lastElement = (model.sprints.Count < pageSize * pageNumber) ? model.sprints.Count : pageSize * pageNumber;

            //despliega la informacion de los usuarios por paginas
            for (int i = (pageNumber - 1) * pageSize; i < lastElement; i++)
            {
                var spr = new ProyectoInfoModel.sprintInfo();
                spr.numero = model.sprints.ElementAt(i).numero.ToString();
                spr.modulos = model.sprints.ElementAt(i).Sprint_Modulos.Count().ToString();
                spr.fechaInicio = model.sprints.ElementAt(i).fechaInicio.ToString("dd/MM/yyyy");
                spr.fechaFinal = model.sprints.ElementAt(i).fechaFinal.ToString("dd/MM/yyyy");
                model.indexSprintInfoList.Add(spr);
            }
            model.crearSprints = revisarPermisos("Crear Sprints");
            model.detallesSprints = revisarPermisos("Consultar Detalles de Proyectos");
            var sprintsAsIPagedList = new StaticPagedList<ProyectoInfoModel.sprintInfo>(model.indexSprintInfoList, pageNumber, pageSize, model.sprints.Count);
            ViewBag.OnePageOfSprints = sprintsAsIPagedList;

            model.indexModuloInfoList = new List<ProyectoInfoModel.moduloInfo>();
            int pageSize2 = 10;
            int pageNumber2 = (moduloPage ?? 1);
            int lastElement2 = (model.sprints.Count < pageSize2 * pageNumber2) ? model.modulos.Count : pageSize2 * pageNumber2;

            //despliega la informacion de los usuarios por paginas
            for (int i = (pageNumber2 - 1) * pageSize2; i < lastElement2; i++)
            {
                var mod = new ProyectoInfoModel.moduloInfo();
                mod.numero = model.modulos.ElementAt(i).numero.ToString();
                mod.nombre = model.modulos.ElementAt(i).nombre;
                mod.requerimientos = model.modulos.ElementAt(i).Requerimientos.Count().ToString();
                model.indexModuloInfoList.Add(mod);
            }
            model.crearModulos = revisarPermisos("Crear Módulos");
            model.detallesModulos = revisarPermisos("Consultar Detalles de Proyectos");
            var modulosAsIPagedList = new StaticPagedList<ProyectoInfoModel.moduloInfo>(model.indexModuloInfoList, pageNumber2, pageSize2, model.modulos.Count);
            ViewBag.OnePageOfModulos = modulosAsIPagedList;
            return View(model);
        }
    }
}