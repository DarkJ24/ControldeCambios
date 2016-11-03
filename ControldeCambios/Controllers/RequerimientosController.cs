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
        public ActionResult Detalles(int? requerimiento)
        {
            if (!revisarPermisos("Consultar Detalles de Requerimiento"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar detalles de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

            if (requerimiento == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequerimientosModelo modeloReq = new RequerimientosModelo();
            UsuariosModelo modeloUsuario = new UsuariosModelo();
            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Modulo> listaModulos = new List<Modulo>();
            List<Estado_Requerimientos> listaEstadoRequerimientos = new List<Estado_Requerimientos>();
            List<Usuario> listaClientes = new List<Usuario>();
            modeloReq.requerimiento = baseDatos.Requerimientos.Find(requerimiento);
            if (modeloReq.requerimiento == null)
            {
                return HttpNotFound();
            }
            modeloReq.id = requerimiento ?? default(int);
            modeloReq.codigo = modeloReq.requerimiento.codigo;
            modeloReq.nombre = modeloReq.requerimiento.nombre;
            modeloReq.version = modeloReq.requerimiento.version.ToString();
            modeloReq.descripcion = modeloReq.requerimiento.descripcion;
            modeloReq.prioridad = modeloReq.requerimiento.prioridad.ToString();
            modeloReq.esfuerzo = modeloReq.requerimiento.esfuerzo.ToString();
            modeloReq.observaciones = modeloReq.requerimiento.observaciones;
            modeloReq.fechaInicial = modeloReq.requerimiento.creadoEn.ToString("MM/dd/yyyy");
            modeloReq.solicitadoPor = modeloReq.requerimiento.solicitadoPor;
            modeloReq.estado = modeloReq.requerimiento.estado;
            ViewBag.modulos = new SelectList(listaModulos, "cedula", "nombre");
            ViewBag.estadoRequerimientos = new SelectList(listaEstadoRequerimientos, "cedula", "nombre");
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

            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre", modeloReq.solicitadoPor);
            ViewBag.desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Usuarios = baseDatos.Usuarios.ToList();

            modeloReq.modificarRequerimiento = revisarPermisos("Modificar Requerimientos");
            modeloReq.eliminarRequerimiento = revisarPermisos("Eliminar Requerimientos");

            return View(modeloReq);
        }

        /// <summary>
        /// Metodo encargado de la modificacion de requerimientos.
        /// </summary>
        /// <param name="id"> Modelo con la informacion del requerimiento para modificar.</param>
        /// <returns>Pagina de Detalles</returns>
        // POST: Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(RequerimientosModelo modeloReq)
        {
            if (ModelState.IsValid)
            {
                var requerimiento = new Requerimiento();
                requerimiento.codigo = modeloReq.requerimiento.codigo;
                requerimiento.nombre = modeloReq.requerimiento.nombre;
                requerimiento.version = modeloReq.requerimiento.version;
                baseDatos.Entry(requerimiento).State = System.Data.Entity.EntityState.Modified;

                baseDatos.SaveChanges();

                //this.AddToastMessage("Requerimiento Modificado", "El requerimiento " + modeloReq.requerimiento.nombre + " se ha actualizado correctamente.", ToastType.Success);
                //return RedirectToAction("Index", "Usuarios");

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

            return View(modeloReq);
        }

        /// <summary>
        /// Funcionalidad de borrar requerimientos.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        /// <param name="model"> Modelo con la informacion del Usuario a borrar.</param>
        //POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Borrar(RequerimientosModelo modeloReq)
        {
            var user = await baseDatos.Requerimientos.FindAsync(modeloReq.requerimiento.codigo);
            baseDatos.Entry(user).State = System.Data.Entity.EntityState.Deleted;
            baseDatos.SaveChanges();

            this.AddToastMessage("Usuario Borrado", "El requerimiento " + modeloReq.requerimiento.nombre + " se ha borrado correctamente.", ToastType.Success);
            return RedirectToAction("Index", "Usuarios");
        }
    }
}