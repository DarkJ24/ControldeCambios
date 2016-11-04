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
using Chart.Mvc;
using Chart.Mvc.ComplexChart;
using PagedList;
using System.Globalization;

namespace ControldeCambios.Controllers
{
    public class RequerimientosController : ToastrController
    {

        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        private void updateSprintPoints(string Proyecto, int sprint)
        {

            var sprint_actual = baseDatos.Sprints.Find(Proyecto, sprint);
            var hoy = DateTime.Today < sprint_actual.fechaInicio ? sprint_actual.fechaInicio : DateTime.Today;

            var progreso_hoy = baseDatos.Progreso_Sprint.Where(x => x.sprintProyecto == Proyecto && x.sprintNumero == sprint &&
            x.fecha.Year == hoy.Year
            && x.fecha.Month == hoy.Month
            && x.fecha.Day == hoy.Day
            ).FirstOrDefault();

            var modificado = true;

            if (progreso_hoy == default(Progreso_Sprint))
            {
                progreso_hoy = new Progreso_Sprint();
                progreso_hoy.fecha = hoy;
                progreso_hoy.sprintNumero = sprint;
                progreso_hoy.sprintProyecto = Proyecto;
                modificado = false;
            }

            var puntos = sprint_actual.Sprint_Modulos
                .Select(m => baseDatos.Modulos
                    .Find(m.proyecto, m.modulo).Requerimientos
                        .Select(x => x.estado == "Finalizado" ? 0 : (x.esfuerzo ?? 0))
                        .Sum())
                .Sum();
            progreso_hoy.puntos = puntos;
            if (modificado)
            {
                baseDatos.Entry(progreso_hoy).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                baseDatos.Progreso_Sprint.Add(progreso_hoy);
            }

            baseDatos.SaveChanges();
        }

        // GET: Requerimientos
        public ActionResult Index(string proyecto, int? page)
        {
            if (!revisarPermisos("Consultar Lista de Requerimientos"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar lista de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            
            if (proyecto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var proy = baseDatos.Proyectos.Find(proyecto);

            if (proy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new RequerimientosIndexModel();
            model.proyecto = proyecto;
            var reqs = baseDatos.Requerimientos.Where(m => m.proyecto == proyecto).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (reqs.Count < pageSize * pageNumber) ? reqs.Count : pageSize * pageNumber;
            model.reqs = new List<Requerimiento>();
            for (int i = (pageNumber - 1) * pageSize; i < lastElement; i++)
            {
                model.reqs.Add(reqs.ElementAt(i));
            }

            var reqsAsIPagedList = new StaticPagedList<Requerimiento>(model.reqs, pageNumber, pageSize, reqs.Count);
            ViewBag.OnePageOfReqs = reqsAsIPagedList;
            model.crearRequerimientos = revisarPermisos("Crear Requerimientos");
            model.detallesRequerimientos = revisarPermisos("Consultar Detalles de Requerimiento");
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
        public ActionResult Crear(string proyecto)
        {
            if (!revisarPermisos("Crear Requerimientos"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (String.IsNullOrEmpty(proyecto))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequerimientosModelo model = new RequerimientosModelo();
            model.proyecto = proyecto;
            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            foreach(var proyEquipo in baseDatos.Proyectos.Find(model.proyecto).Proyecto_Equipo){
                listaDesarrolladores.Add(baseDatos.Usuarios.Find(proyEquipo.usuario));
            }
            foreach (var user in context.Users.ToArray())
            {
                if (user.Roles.First().RoleId.Equals(clienteRol))
                {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                }
            }
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.EstadoRequerimiento = new SelectList(baseDatos.Estado_Requerimientos.ToList(), "nombre", "nombre");
            return View(model);
        }


        /// <summary>
        /// Funcionalidad para crear Requerimientos.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Requerimiento a crear.</param>
        /// <returns>Pagina de Index</returns>
        // POST: /Requerimientos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(RequerimientosModelo model)
        {
            if (ModelState.IsValid)
            {
                var requerimiento = new Requerimiento();
                requerimiento.codigo = model.codigo;
                requerimiento.nombre = model.nombre;
                requerimiento.descripcion = model.descripcion;
                requerimiento.prioridad = Int32.Parse(model.prioridad);
                requerimiento.esfuerzo = Int32.Parse(model.esfuerzo);
                requerimiento.creadoEn = DateTime.ParseExact(model.fechaInicial, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                requerimiento.finalizaEn = DateTime.ParseExact(model.fechaFinal, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                requerimiento.observaciones = model.observaciones;
                requerimiento.solicitadoPor = model.solicitadoPor;
                requerimiento.creadoPor = model.creadoPor;
                requerimiento.estado = model.estado;
                requerimiento.proyecto = model.proyecto;
                requerimiento.Usuarios = model.equipo.Select(x => baseDatos.Usuarios.Find(x)).ToList();

                var criterios = model.criteriosAceptacion.Split('|').ToList();

                var criterio_list = new List<Requerimientos_Cri_Acep>();
                foreach(var criterio in criterios)
                {
                    var cri_ac = new Requerimientos_Cri_Acep();
                    cri_ac.criterio = criterio;
                    criterio_list.Add(cri_ac);
                }

                requerimiento.Requerimientos_Cri_Acep = criterio_list;
                baseDatos.Requerimientos.Add(requerimiento);

                baseDatos.SaveChanges();
                this.AddToastMessage("Requerimiento Creado", "El requerimiento " + model.nombre + " se ha creado correctamente.", ToastType.Success);
                return RedirectToAction("Crear", "Requerimientos", new { proyecto = model.proyecto });
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
            var listaModulos = baseDatos.Modulos.Where(m => m.proyecto == model.proyecto).ToList();
            ViewBag.Modulo = new SelectList(listaModulos, "numero", "nombre");

            ViewBag.EstadoRequerimiento = new SelectList(baseDatos.Estado_Requerimientos.ToList(), "nombre", "nombre");

            return View(model);
        }

        /// <summary>
        /// Funcionalidad para consultar los detalles de los requerimientos con base en un id.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        // GET: /Requerimientos/Detalles
        // GET: Detalles
        public ActionResult Detalles(int id)
        {
            if (!revisarPermisos("Modificar Requerimientos"))
            {
                //Despliega mensaje en caso de no poder modificar un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para modificar requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequerimientosModelo modelo = new RequerimientosModelo();
            modelo.requerimiento = baseDatos.Requerimientos.Find(id);
            if (modelo.requerimiento.id == null)
            {
                return HttpNotFound();
            }
            modelo.id = modelo.requerimiento.id;
            modelo.codigo = modelo.requerimiento.codigo;
            modelo.nombre = modelo.requerimiento.nombre;
            modelo.creadoPor = modelo.requerimiento.creadoPor;
            modelo.version = modelo.requerimiento.version.ToString();
            modelo.descripcion = modelo.requerimiento.descripcion;
            modelo.prioridad = modelo.requerimiento.prioridad.ToString();
            modelo.esfuerzo = modelo.requerimiento.esfuerzo.ToString();
            modelo.observaciones = modelo.requerimiento.observaciones;
            modelo.fechaInicial = modelo.requerimiento.creadoEn.ToString("MM/dd/yyyy");
            modelo.solicitadoPor = modelo.requerimiento.solicitadoPor;
            modelo.estado = modelo.requerimiento.estado;
            modelo.proyecto = modelo.requerimiento.proyecto;

            var requerimiento = baseDatos.Requerimientos.Find(id);
            modelo.requerimiento = requerimiento;

            modelo.criteriosAceptacion = requerimiento.Requerimientos_Cri_Acep.Select(c => c.criterio).Aggregate((acc, x) => acc + "|" + x);

            /*var equipo = proyecto.Proyecto_Equipo.ToList();
            model.equipo = new List<string>();
            foreach (var des in equipo)
            {
                model.equipo.Add(des.usuario);
            }*/

            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Modulo> listaModulos = new List<Modulo>();
            List<Estado_Requerimientos> listaEstadoRequerimientos = new List<Estado_Requerimientos>();
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

            modelo.eliminarRequerimiento = revisarPermisos("Eliminar Requerimientos");
            modelo.modificarRequerimiento = revisarPermisos("Modificar Requerimientos");
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");
            return View(modelo);
        }

        /// <summary>
        /// Metodo encargado de la modificacion de requerimientos.
        /// </summary>
        /// <param name="id"> Modelo con la informacion del requerimiento para modificar.</param>
        /// <returns>Pagina de Detalles</returns>
        // POST: Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(RequerimientosModelo modelo)
        {
            if (ModelState.IsValid)
            {
                var requerimiento = new Requerimiento();
                requerimiento.codigo = modelo.codigo;
                requerimiento.version = Int32.Parse(modelo.version);
                requerimiento.creadoPor = modelo.creadoPor;
                requerimiento.descripcion = modelo.descripcion;
                requerimiento.solicitadoPor = modelo.solicitadoPor;
                requerimiento.prioridad = Int32.Parse(modelo.prioridad);
                requerimiento.esfuerzo = Int32.Parse(modelo.esfuerzo);
                requerimiento.creadoEn = DateTime.ParseExact(modelo.fechaInicial, "MM/dd/yyyy", null);
                if (modelo.fechaFinal != null)
                {
                    requerimiento.finalizaEn = DateTime.ParseExact(modelo.fechaFinal, "MM/dd/yyyy", null);
                }                
                requerimiento.estado = modelo.estado;
                requerimiento.observaciones = modelo.observaciones;
                requerimiento.proyecto = modelo.proyecto;
                baseDatos.Entry(requerimiento).State = System.Data.Entity.EntityState.Modified;

                /*var equipo_viejo = baseDatos.Proyecto_Equipo.Where(m => m.proyecto == model.nombre).ToList();

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
                }*/

                baseDatos.SaveChanges();
                this.AddToastMessage("Requerimiento Modificado", "El requerimiento " + modelo.nombre + " se ha modificado correctamente.", ToastType.Success);
                return RedirectToAction("Detalles", "Requerimientos", new { id = requerimiento.id });

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
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre"); ;

            return View(modelo);
        }

        /// <summary>
        /// Funcionalidad de borrar requerimientos.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        /// <param name="model"> Modelo con la informacion del Usuario a borrar.</param>
        //POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Borrar(RequerimientosModelo modeloReq)
        {
            var criterios = baseDatos.Requerimientos_Cri_Acep.Where(m => m.idReq == modeloReq.requerimiento.id).ToList();
            baseDatos.Entry(criterios).State = System.Data.Entity.EntityState.Deleted;
            var req = baseDatos.Requerimientos.Find(modeloReq.requerimiento.id);
            baseDatos.Entry(req).State = System.Data.Entity.EntityState.Deleted;
            baseDatos.SaveChanges();

            this.AddToastMessage("Usuario Borrado", "El requerimiento " + modeloReq.requerimiento.nombre + " se ha borrado correctamente.", ToastType.Success);
            return RedirectToAction("Index", "Home");
        }
    }
}