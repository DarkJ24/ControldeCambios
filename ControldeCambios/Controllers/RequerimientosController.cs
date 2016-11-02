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
        public ActionResult Index(string proyecto, int? sprint, int? page)
        {
            // si no se usó un parámetro para el proyecto se manda este mensaje
            if(proyecto == null)
            {
                this.AddToastMessage("Error", "No se encontró o no se tiene permisos para ver el proyecto", ToastType.Error);
                return RedirectToAction("Index", "Home");
            }

            var proy = baseDatos.Proyectos.Find(proyecto);

            //si el proyecto es invalido se manda un mensaje
            if(proy == null)
            {
                this.AddToastMessage("Error", "No se encontró o no se tiene permisos para ver el proyecto", ToastType.Error);
                return RedirectToAction("Index", "Home");
            }

            // conseguimos el numero de sprint actual
            int ssprint = sprint ?? 1;
            
            //conseguimos la lista de los sprints del proyecto para hacer paginacion
            var sprints_de_proyecto = baseDatos.Sprints.Where(s => s.proyecto == proyecto).Select(s => s.numero).ToList();

            // montamos el modelo para la pagina
            var model = new RequerimientosIndexModel();

            model.titulo = proy.nombre;
            model.descripcion = proy.descripcion;
            model.sprints = sprints_de_proyecto;
            model.id = proyecto;
            model.sprint = ssprint;

            var cliente = baseDatos.Usuarios.Find(proy.cliente);
            var lider = baseDatos.Usuarios.Find(proy.lider);

            model.cliente = cliente.nombre;
            model.lider = lider.nombre;

            var sprint_modulos = baseDatos.Sprints.Find(proyecto, ssprint).Sprint_Modulos;
            var reqs = sprint_modulos.Select(m => m.Modulo1.Requerimientos.ToList()).Aggregate((agg, m) => agg.Concat(m).ToList()).ToList();


            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (reqs.Count < pageSize * pageNumber) ? reqs.Count : pageSize * pageNumber;

            model.reqs = new List<Requerimiento>();

            //despliega la informacion de los usuarios por paginas
            for (int i = (pageNumber - 1) * pageSize; i < lastElement; i++)
            {
                model.reqs.Add(reqs.ElementAt(i));
            }

            var reqsAsIPagedList = new StaticPagedList<Requerimiento>(model.reqs, pageNumber, pageSize, reqs.Count);
            ViewBag.OnePageOfReqs = reqsAsIPagedList;

            var sprint_actual = baseDatos.Sprints.Find(proyecto, ssprint);

            //charts
            DateTime start = sprint_actual.fechaInicio;
            DateTime end = sprint_actual.fechaFinal;

            ViewBag.dias = Enumerable.Range(0, 1 + end.Subtract(start).Days).Select(offset => start.AddDays(offset).ToString("dd/MM/yy")).ToArray();

            // extrapolar los valores que no se encuentran en la base de datos
            var horas_esfuerzo = baseDatos.Progreso_Sprint.Where(s => s.sprintNumero == ssprint && s.sprintProyecto == proyecto).ToList();
            
            var esfuerzo_real = new List<double>();
            if (horas_esfuerzo.Any()) {
                var ultimo_dia = horas_esfuerzo.Select(h => h.fecha).Max();
                for (var dia = start; dia < ultimo_dia.AddDays(1); dia = dia.AddDays(1))
                {
                    double puntaje;
                    var esfuerzo_actual = horas_esfuerzo.Where(s => s.fecha.Date.Equals(dia.Date)).FirstOrDefault();
                    if(esfuerzo_actual == null)
                    {
                        puntaje = esfuerzo_real.Last();
                    } else
                    {
                        puntaje = esfuerzo_actual.puntos;
                    }
                    esfuerzo_real.Add(puntaje);
                }

                

            }

            ViewBag.esfuerzo_real = esfuerzo_real;

            var esfuerzo_total = baseDatos.Progreso_Sprint.Find(start, proyecto, ssprint);

            // mostrar una estumacion del esfuerzo ideal si se encuentran datos acerca de los puntajes
            if(esfuerzo_total != null)
            {
                var puntos = esfuerzo_total.puntos;
                double longitud_del_sprint = end.Subtract(start).TotalDays;
                var velocidad = puntos / longitud_del_sprint;

                ViewBag.esfuerzo_ideal = Enumerable.Range(0, (int)longitud_del_sprint+1).Select(x => System.Convert.ToDouble(((int)longitud_del_sprint - x) *velocidad)).ToList();

            } else
            {
                ViewBag.esfuerzo_ideal = new List<double>();
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
        public ActionResult Crear(string proyecto)
        {
            if (!revisarPermisos("Crear Requerimientos"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear requerimientos!", ToastType.Warning);
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
            var listaModulos = baseDatos.Modulos.Where(m => m.proyecto == proyecto).ToList();
            ViewBag.Modulo = new SelectList(listaModulos, "numero", "nombre");
            ViewBag.EstadoRequerimiento = new SelectList(baseDatos.Estado_Requerimientos.ToList(), "nombre", "nombre");

            RequerimientosModelo model = new RequerimientosModelo();

            model.proyecto = proyecto;

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
                requerimiento.modulo = Int32.Parse(model.modulo);
                requerimiento.estado = model.estado;
                

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
        public ActionResult Detalles(string requerimiento, int version)
        {
            if (!revisarPermisos("Consultar Detalles de Requerimiento"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar detalles de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

            if (String.IsNullOrEmpty(requerimiento))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequerimientosModelo modeloReq = new RequerimientosModelo();
            UsuariosModelo modeloUsuario = new UsuariosModelo();
            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Modulo> listaModulos = new List<Modulo>();
            List<Estado_Requerimientos> listaEstadoRequerimientos = new List<Estado_Requerimientos>();
            List<Usuario> listaClientes = new List<Usuario>();
            modeloReq.requerimiento = baseDatos.Requerimientos.Find(requerimiento, version);
            if (modeloReq.requerimiento == null)
            {
                return HttpNotFound();
            }
            modeloReq.codigo = modeloReq.requerimiento.codigo;
            modeloReq.nombre = modeloReq.requerimiento.nombre;
            modeloReq.descripcion = modeloReq.requerimiento.descripcion;
            modeloReq.prioridad = modeloReq.requerimiento.prioridad.ToString();
            modeloReq.esfuerzo = modeloReq.requerimiento.esfuerzo.ToString();
            modeloReq.observaciones = modeloReq.requerimiento.observaciones;
            modeloReq.fechaInicial = modeloReq.requerimiento.creadoEn.ToString("MM/dd/yyyy");
            modeloReq.solicitadoPor = modeloReq.requerimiento.solicitadoPor;
            //modeloReq.modulo = 
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
            return View(modeloReq);
        }
    }
}