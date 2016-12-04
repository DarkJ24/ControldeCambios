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
using System.Text;

namespace ControldeCambios.Controllers
{
    public class Solicitud_CambiosController : ToastrController
    {

        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        // GET: Solicitud_Cambios
        public ActionResult Index(string proyecto, int? page)
        {
            /*if (!revisarPermisos("Consultar Lista de Requerimientos"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar la lista de versiones de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }*/

            if (proyecto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var proy = baseDatos.Proyectos.Find(proyecto);

            if (proy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new Solicitud_CambiosIndexModel();
            model.proyecto = proyecto;
            String userIdentityId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            String usuarioActual = baseDatos.Usuarios.Where(m => m.id == userIdentityId).First().cedula;
            var solicitudes = baseDatos.Solicitud_Cambios.Where(m => m.proyecto == proyecto && m.solicitadoPor == usuarioActual).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (solicitudes.Count < pageSize * pageNumber) ? solicitudes.Count : pageSize * pageNumber;
            model.indexSolicitudInfoList = new List<Solicitud_CambiosIndexModel.solicitudInfo>();
            for (int i = (pageNumber - 1) * pageSize; i < lastElement; i++)
            {
                if (solicitudes.ElementAt(i).tipo == "Modificar")
                {
                    var requerimiento = baseDatos.Requerimientos.Find(solicitudes.ElementAt(i).req2);
                    var x = new Solicitud_CambiosIndexModel.solicitudInfo();
                    x.id = solicitudes.ElementAt(i).id;
                    x.nombre = requerimiento.nombre;
                    x.codigo = requerimiento.codigo;
                    x.estado = solicitudes.ElementAt(i).estado;
                    x.fecha = solicitudes.ElementAt(i).solicitadoEn.ToString("MM/dd/yyyy");
                    x.prioridad = requerimiento.prioridad;
                    x.solicitadoPor = baseDatos.Usuarios.Find(solicitudes.ElementAt(i).solicitadoPor).nombre;
                    x.tipo = solicitudes.ElementAt(i).tipo;
                    model.indexSolicitudInfoList.Add(x);
                } else { //Eliminar
                    var requerimiento = baseDatos.Requerimientos.Find(solicitudes.ElementAt(i).req1);
                    var x = new Solicitud_CambiosIndexModel.solicitudInfo();
                    x.id = solicitudes.ElementAt(i).id;
                    x.nombre = requerimiento.nombre;
                    x.codigo = requerimiento.codigo;
                    x.estado = solicitudes.ElementAt(i).estado;
                    x.fecha = solicitudes.ElementAt(i).solicitadoEn.ToString("MM/dd/yyyy");
                    x.prioridad = requerimiento.prioridad;
                    x.solicitadoPor = baseDatos.Usuarios.Find(solicitudes.ElementAt(i).solicitadoPor).nombre;
                    x.tipo = solicitudes.ElementAt(i).tipo;
                    model.indexSolicitudInfoList.Add(x);
                }
            }
            var reqsAsIPagedList = new StaticPagedList<Solicitud_CambiosIndexModel.solicitudInfo>(model.indexSolicitudInfoList, pageNumber, pageSize, solicitudes.Count);
            ViewBag.OnePageOfReqs = reqsAsIPagedList;
            //model.crearRequerimientos = revisarPermisos("Crear Requerimientos");
            //model.detallesRequerimientos = revisarPermisos("Consultar Detalles de Requerimiento");
            return View(model);
        }

        // GET: Solicitud_Cambios
        public ActionResult IndexAprobacion(string proyecto, int? page)
        {
            /*if (!revisarPermisos("Consultar Lista de Requerimientos"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar la lista de versiones de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }*/

            if (proyecto == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var proy = baseDatos.Proyectos.Find(proyecto);

            if (proy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new Solicitud_CambiosIndexModel();
            model.proyecto = proyecto;
            var solicitudes = baseDatos.Solicitud_Cambios.Where(m => m.proyecto == proyecto).ToList();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (solicitudes.Count < pageSize * pageNumber) ? solicitudes.Count : pageSize * pageNumber;
            model.indexSolicitudInfoList = new List<Solicitud_CambiosIndexModel.solicitudInfo>();
            for (int i = (pageNumber - 1) * pageSize; i < lastElement; i++)
            {
                if (solicitudes.ElementAt(i).tipo == "Modificar")
                {
                    var requerimiento = baseDatos.Requerimientos.Find(solicitudes.ElementAt(i).req2);
                    var x = new Solicitud_CambiosIndexModel.solicitudInfo();
                    x.id = solicitudes.ElementAt(i).id;
                    x.nombre = requerimiento.nombre;
                    x.codigo = requerimiento.codigo;
                    x.estado = solicitudes.ElementAt(i).estado;
                    x.fecha = solicitudes.ElementAt(i).solicitadoEn.ToString("MM/dd/yyyy");
                    x.prioridad = requerimiento.prioridad;
                    x.solicitadoPor = baseDatos.Usuarios.Find(solicitudes.ElementAt(i).solicitadoPor).nombre;
                    x.tipo = solicitudes.ElementAt(i).tipo;
                    model.indexSolicitudInfoList.Add(x);
                }
                else
                { //Eliminar
                    var requerimiento = baseDatos.Requerimientos.Find(solicitudes.ElementAt(i).req1);
                    var x = new Solicitud_CambiosIndexModel.solicitudInfo();
                    x.id = solicitudes.ElementAt(i).id;
                    x.nombre = requerimiento.nombre;
                    x.codigo = requerimiento.codigo;
                    x.estado = solicitudes.ElementAt(i).estado;
                    x.fecha = solicitudes.ElementAt(i).solicitadoEn.ToString("MM/dd/yyyy");
                    x.prioridad = requerimiento.prioridad;
                    x.solicitadoPor = baseDatos.Usuarios.Find(solicitudes.ElementAt(i).solicitadoPor).nombre;
                    x.tipo = solicitudes.ElementAt(i).tipo;
                    model.indexSolicitudInfoList.Add(x);
                }
            }
            var reqsAsIPagedList = new StaticPagedList<Solicitud_CambiosIndexModel.solicitudInfo>(model.indexSolicitudInfoList, pageNumber, pageSize, solicitudes.Count);
            ViewBag.OnePageOfReqs = reqsAsIPagedList;
            //model.crearRequerimientos = revisarPermisos("Crear Requerimientos");
            //model.detallesRequerimientos = revisarPermisos("Consultar Detalles de Requerimiento");
            return View(model);
        }


        /// <summary>
        /// Funcionalidad para llenar los datos del requerimiento actual y la solicitud de cambio del requerimiento.
        /// </summary>
        // GET: /Solicitud_Cambios/Aprobar
        public ActionResult Aprobar(string id)
        {
            /*if (!revisarPermisos("Aprobar Solicitud de Cambio"))   // Revisa los permisos del usuario accediendo a la pantalla
            {
                //Despliega mensaje en caso de no poder modificar un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para ver detalles de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }*/
            if (id == null) // Si no existe el requerimiento, redirecciona a error
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AprobarSolicitudCambioModelo modelo = new AprobarSolicitudCambioModelo();   // Crea un modelo y lo llena con los datos del requerimiento
            var solicitud = baseDatos.Solicitud_Cambios.Find(Int32.Parse(id));   // que entro como parametro
            if (solicitud == null)
            {
                return HttpNotFound();
            }

            modelo.id = id;
            
            var req1 = baseDatos.Requerimientos.Find(solicitud.req1);
            if (solicitud.tipo == "Modificar")
            {
                var req2 = baseDatos.Requerimientos.Find(solicitud.req2);
                modelo.id2 = req2.id;
                modelo.codigo2 = req2.codigo;
                modelo.nombre2 = req2.nombre;
                modelo.creadoPor2 = req2.creadoPor;
                modelo.version2 = req2.version.ToString();
                modelo.descripcion2 = req2.descripcion;
                modelo.prioridad2 = req2.prioridad.ToString();
                modelo.esfuerzo2 = req2.esfuerzo.ToString();
                modelo.observaciones2 = req2.observaciones;
                modelo.fechaInicial2 = req2.creadoEn.ToString("MM/dd/yyyy");
                if (req2.finalizaEn != null)
                {
                    modelo.fechaFinal2 = (req2.finalizaEn ?? DateTime.Now).ToString("MM/dd/yyyy");
                }
                modelo.solicitadoPor2 = req2.solicitadoPor;
                modelo.estado2 = req2.estado;
                if (req2.imagen != null)
                {
                    modelo.file2 = HttpUtility.UrlEncode(Convert.ToBase64String(req2.imagen));
                }
                else
                {
                    modelo.file2 = "";
                }
                modelo.equipo2 = new List<string>();     // Se llena la variable equipo con el equipo ya asignado a este requerimiento, si ya hay uno
                foreach (var des in req2.Usuarios.ToList())
                {
                    modelo.equipo2.Add(des.cedula);
                }
                modelo.criteriosAceptacion2 = req2.Requerimientos_Cri_Acep.Select(c => c.criterio).Aggregate((acc, x) => acc + "|" + x);
            }
            modelo.id1 = req1.id; // Diferentes asignaciones de variables para el modelo creado
            modelo.codigo1 = req1.codigo;
            modelo.nombre1 = req1.nombre;
            modelo.creadoPor1 = baseDatos.Usuarios.Find(req1.creadoPor).nombre;
            modelo.version1 = req1.version.ToString();
            modelo.descripcion1 = req1.descripcion;
            modelo.prioridad1 = req1.prioridad.ToString();
            modelo.esfuerzo1 = req1.esfuerzo.ToString();
            modelo.observaciones1 = req1.observaciones;
            modelo.fechaInicial1 = req1.creadoEn.ToString("MM/dd/yyyy");
            if (req1.finalizaEn != null)
            {
                modelo.fechaFinal1 = (req1.finalizaEn ?? DateTime.Now).ToString("MM/dd/yyyy");
            }
            modelo.solicitadoPor1 = baseDatos.Usuarios.Find(req1.solicitadoPor).nombre;
            modelo.estado1 = req1.estado;
            modelo.proyecto = req1.proyecto;
            modelo.tipo = solicitud.tipo;
            modelo.estado = solicitud.estado;
            if (solicitud.aprobadoEn != null)
            {
                modelo.aprobadoEn = (solicitud.aprobadoEn ?? DateTime.Now).ToString("MM/dd/yyyy");
            }
            if (!String.IsNullOrWhiteSpace(solicitud.aprobadoPor)) {
                modelo.aprobadoPor = baseDatos.Usuarios.Find(solicitud.aprobadoPor).nombre;
            }
            modelo.solicitadoEn = solicitud.solicitadoEn.ToString("MM/dd/yyyy");
            modelo.solicitadoPor = baseDatos.Usuarios.Find(solicitud.solicitadoPor).nombre;
            modelo.razon = solicitud.razon;
            modelo.comentario = solicitud.comentario;
            if (req1.imagen != null)
            {
                modelo.file1 = HttpUtility.UrlEncode(Convert.ToBase64String(req1.imagen));
            }
            else
            {
                modelo.file1 = "";
            }
            modelo.equipo1 = new List<string>();     // Se llena la variable equipo con el equipo ya asignado a este requerimiento, si ya hay uno
            foreach (var des in req1.Usuarios.ToList())
            {
                modelo.equipo1.Add(des.cedula);
            }
            modelo.criteriosAceptacion1 = req1.Requerimientos_Cri_Acep.Select(c => c.criterio).Aggregate((acc, x) => acc + "|" + x);    // Se agrega a la lista de criterios de aceptacion 
            
            List<Usuario> listaDesarrolladores = new List<Usuario>();       // Se inicializan listas que se usan a traves a continuacion
            List<Modulo> listaModulos = new List<Modulo>();
            List<Estado_Requerimientos> listaEstadoRequerimientos = new List<Estado_Requerimientos>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            //Requerido para formar el equipo de trabajo
            foreach (var proyEquipo in baseDatos.Proyectos.Find(req1.proyecto).Proyecto_Equipo)
            {
                listaDesarrolladores.Add(baseDatos.Usuarios.Find(proyEquipo.usuario));
            }
            foreach (var user in context.Users.ToArray())
            {
                if (user.Roles.First().RoleId.Equals(clienteRol))
                {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                }
            }
            ViewBag.eliminarRequerimiento = revisarPermisos("Eliminar Requerimientos");// Aqui se hacen unas validaciones de permisos 
            ViewBag.modificarRequerimiento = revisarPermisos("Modificar Requerimientos");// y se cargan ciertos Viewbags necesitados por la vista
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");
            return View(modelo);        // Se retorna la vista al modelo luego de cargar los datos
        }


        /// <summary>
        /// Funcionalidad para Aprobar o Rechazar una Solicitud de Cambio.
        /// @param comando: Tipo (Aprobar o Rechazar)
        /// </summary>
        // POST: /Solicitud_Cambios/Aprobar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aprobar(AprobarSolicitudCambioModelo model, string comando, HttpPostedFileBase ImageData)
        {
            var solicitud = baseDatos.Solicitud_Cambios.Find(Int32.Parse(model.id));
            if (ModelState.IsValid) { //Tipo Modificar y Valido
                var req1 = baseDatos.Requerimientos.Find(solicitud.req1);
                var req2 = baseDatos.Requerimientos.Find(solicitud.req2);
                if (comando == "Aprobar") {
                    solicitud.estado = "Aprobado";
                    req1.categoria = "Historial";
                    req2.categoria = "Actual";
                    baseDatos.Entry(req1).State = System.Data.Entity.EntityState.Modified;
                } else {
                    solicitud.estado = "Rechazado";
                    req2.categoria = "Rechazada";
                    //Falta buscar el req actual con la ultima version (en caso de hacer solicitud de una version anterior a la actual)
                    req2.version = req1.version + 1;
                    req2.codigo = model.codigo2;
                    req2.nombre = model.nombre2;
                    req2.prioridad = Int32.Parse(model.prioridad2);
                    req2.observaciones = model.observaciones2;
                    req2.creadoEn = DateTime.ParseExact(model.fechaInicial2, "MM/dd/yyyy", null);
                    req2.creadoPor = model.creadoPor2;
                    req2.descripcion = model.descripcion2;
                    req2.esfuerzo = Int32.Parse(model.esfuerzo2);
                    req2.estado = model.estado2;
                    req2.finalizaEn = DateTime.ParseExact(model.fechaFinal2, "MM/dd/yyyy", null);
                    req2.modulo = req1.modulo;
                    req2.proyecto = req1.proyecto;
                    req2.solicitadoPor = model.solicitadoPor2;
                    if (ImageData != null)
                    {
                        var array = new Byte[ImageData.ContentLength];
                        ImageData.InputStream.Position = 0;
                        ImageData.InputStream.Read(array, 0, ImageData.ContentLength);
                        req2.imagen = array;
                    }
                    else
                    {
                        if (model.file2 == "")
                        {
                            req2.imagen = null;
                        }
                        else
                        {
                            req2.imagen = Encoding.ASCII.GetBytes(model.file2);
                        }
                    }
                }
                String userIdentityId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                String usuarioActual = baseDatos.Usuarios.Where(m => m.id == userIdentityId).First().cedula;
                solicitud.aprobadoPor = usuarioActual;
                solicitud.aprobadoEn = DateTime.Now;
                solicitud.comentario = model.comentario;
                baseDatos.Entry(req2).State = System.Data.Entity.EntityState.Modified;
                baseDatos.Entry(solicitud).State = System.Data.Entity.EntityState.Modified;
                baseDatos.SaveChanges();
                this.AddToastMessage("Solicitud Aprobada", "La solicitud de modificar " + req2.nombre + " ha sido aprobada.", ToastType.Success);
                return RedirectToAction("indexAprobacion", "Solicitud_Cambios", new { proyecto = solicitud.proyecto });
            }
            if (model.tipo == "Eliminar")
            {
                if (!String.IsNullOrWhiteSpace(model.comentario))
                {
                    var req1 = baseDatos.Requerimientos.Find(solicitud.req1);
                    if (comando == "Aprobar") {
                        solicitud.estado = "Aprobado";
                        req1.categoria = "Historial";
                        baseDatos.Entry(req1).State = System.Data.Entity.EntityState.Modified;
                    } else {
                        solicitud.estado = "Rechazado";
                    }
                    String userIdentityId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    String usuarioActual = baseDatos.Usuarios.Where(m => m.id == userIdentityId).First().cedula;
                    solicitud.aprobadoPor = usuarioActual;
                    solicitud.aprobadoEn = DateTime.Now;
                    solicitud.comentario = model.comentario;
                    baseDatos.Entry(solicitud).State = System.Data.Entity.EntityState.Modified;
                    baseDatos.SaveChanges();
                    this.AddToastMessage("Solicitud Aprobada", "La solicitud de eliminar " + req1.nombre + " ha sido aprobada.", ToastType.Success);
                    return RedirectToAction("indexAprobacion", "Solicitud_Cambios", new { proyecto = solicitud.proyecto });
                }
                //No puso comentario
                this.AddToastMessage("Error al "+comando+" la solicitud", "Se necesita un comentario para aprobar o rechazar la solicitud.", ToastType.Error);
                return RedirectToAction("Aprobar", "Solicitud_Cambios", new { id = model.id });
            }
            //Hay algo malo en el modelo, reprocesar vista
            List<Usuario> listaDesarrolladores = new List<Usuario>();       // Se inicializan listas que se usan a traves a continuacion
            List<Modulo> listaModulos = new List<Modulo>();
            List<Estado_Requerimientos> listaEstadoRequerimientos = new List<Estado_Requerimientos>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            //Requerido para formar el equipo de trabajo
            foreach (var proyEquipo in baseDatos.Proyectos.Find(solicitud.proyecto).Proyecto_Equipo)
            {
                listaDesarrolladores.Add(baseDatos.Usuarios.Find(proyEquipo.usuario));
            }
            foreach (var user in context.Users.ToArray())
            {
                if (user.Roles.First().RoleId.Equals(clienteRol))
                {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                }
            }
            ViewBag.eliminarRequerimiento = revisarPermisos("Eliminar Requerimientos");// Aqui se hacen unas validaciones de permisos 
            ViewBag.modificarRequerimiento = revisarPermisos("Modificar Requerimientos");// y se cargan ciertos Viewbags necesitados por la vista
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");
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

        // GET: Crear Solicitud_Cambios
        public ActionResult CrearSolicitud(string id, int? page)
        {
            if (!revisarPermisos("Consultar Detalles de Requerimiento"))   // Revisa los permisos del usuario accediendo a la pantalla
            {
                //Despliega mensaje en caso de no poder modificar un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para ver detalles de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (id == null) // Si no existe el requerimiento, redirecciona a error
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CrearSolicitudModel modelo = new CrearSolicitudModel();   // Crea un modelo y lo llena con los datos del requerimiento
            var requerimiento = baseDatos.Requerimientos.Find(Int32.Parse(id));   // que entro como parametro
            if (requerimiento == null)
            {
                return HttpNotFound();
            }
            modelo.idReqAnterior = requerimiento.id;             // Diferentes asignaciones de variables para el modelo creado
            modelo.codigo = requerimiento.codigo;
            modelo.nombre = requerimiento.nombre;
            modelo.creadoPor = requerimiento.creadoPor;
            modelo.version = requerimiento.version.ToString();
            modelo.descripcion = requerimiento.descripcion;
            modelo.prioridad = requerimiento.prioridad.ToString();
            modelo.esfuerzo = requerimiento.esfuerzo.ToString();
            modelo.observaciones = requerimiento.observaciones;
            modelo.fechaInicial = requerimiento.creadoEn.ToString("MM/dd/yyyy");
            if (requerimiento.finalizaEn != null)
            {
                modelo.fechaFinal = (requerimiento.finalizaEn ?? DateTime.Now).ToString("MM/dd/yyyy");
            }
            modelo.solicitadoPor = requerimiento.solicitadoPor;
            modelo.estado = requerimiento.estado;
            modelo.proyecto = requerimiento.proyecto;

            if (requerimiento.imagen != null)
            {
                modelo.file = HttpUtility.UrlEncode(Convert.ToBase64String(requerimiento.imagen));
            }
            else
            {
                modelo.file = "";
            }

            modelo.equipo = new List<string>();     // Se llena la variable equipo con el equipo ya asignado a este requerimiento, si ya hay uno
            foreach (var des in requerimiento.Usuarios.ToList())
            {
                modelo.equipo.Add(des.cedula);
            }

            modelo.criteriosAceptacion = requerimiento.Requerimientos_Cri_Acep.Select(c => c.criterio).Aggregate((acc, x) => acc + "|" + x);    // Se agrega a la lista de criterios de aceptacion 
                                                                                                                                                // los que ya estan vinculados con este requerimiento
            List<Usuario> listaDesarrolladores = new List<Usuario>();       // Se inicializan listas que se usan a traves a continuacion
            List<Modulo> listaModulos = new List<Modulo>();
            List<Estado_Requerimientos> listaEstadoRequerimientos = new List<Estado_Requerimientos>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            //Requerido para formar el equipo de trabajo
            foreach (var proyEquipo in baseDatos.Proyectos.Find(requerimiento.proyecto).Proyecto_Equipo)
            {
                listaDesarrolladores.Add(baseDatos.Usuarios.Find(proyEquipo.usuario));
            }
            foreach (var user in context.Users.ToArray())
            {
                if (user.Roles.First().RoleId.Equals(clienteRol))
                {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                }
            }
            modelo.eliminarRequerimiento = revisarPermisos("Eliminar Requerimientos");              // Aqui se hacen unas validaciones de permisos 
            modelo.crearSolicitud = revisarPermisos("Modificar Requerimientos");            // y se cargan ciertos Viewbags necesitados por la vista
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");

            //Indice de Versiones Anteriores
            var solicitudes = baseDatos.Solicitud_Cambios.Where(m => m.proyecto == requerimiento.proyecto && m.estado == "Aprobado" && (m.req1 == requerimiento.id || m.req2 == requerimiento.id)).ToList();
            solicitudes = getRequerimientos(solicitudes);
            var reqs = new List<Requerimiento>();
            foreach (var sol in solicitudes)
            {
                var req = baseDatos.Requerimientos.Find(sol.req1);
                if (req.categoria == "Historial")
                {
                    if (!reqs.Contains(req))
                    {
                        reqs.Add(req);
                    }
                }
                var req2 = baseDatos.Requerimientos.Find(sol.req2);
                if (req2.categoria == "Historial" || req2.categoria == "Actual")
                {
                    if (!reqs.Contains(req2))
                    {
                        reqs.Add(req2);
                    }
                }
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (reqs.Count < pageSize * pageNumber) ? reqs.Count : pageSize * pageNumber;
            modelo.reqs = new List<Requerimiento>();
            for (int i = (pageNumber - 1) * pageSize; i < lastElement; i++)
            {
                modelo.reqs.Add(reqs.ElementAt(i));
            }

            var reqsAsIPagedList = new StaticPagedList<Requerimiento>(modelo.reqs, pageNumber, pageSize, reqs.Count);
            ViewBag.OnePageOfReqs = reqsAsIPagedList;

            return View(modelo);        // Se retorna la vista al modelo luego de cargar los datos
        }

        // POST: Crear Solicitud de Cambio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearSolicitud(CrearSolicitudModel modelo, HttpPostedFileBase ImageData)
        {
            if (ModelState.IsValid)     // Verifica si el modelo que entra como parametro es valido para modificar
            {
                var requerimientoViejo = baseDatos.Requerimientos.Find(modelo.idReqAnterior);   // Se busca el modelo en la base y se cambian sus datos por los
                var requerimiento = new Requerimiento();
                requerimiento.nombre = modelo.nombre;                           // del modelo que entra como parametro
                requerimiento.codigo = modelo.codigo;
                requerimiento.version = requerimientoViejo.version + 1;
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
                requerimiento.Usuarios = new List<Usuario>();
                baseDatos.SaveChanges();
                if (modelo.equipo != null)
                {
                    foreach (var desarrollador in modelo.equipo)
                    {
                        requerimiento.Usuarios.Add(baseDatos.Usuarios.Find(desarrollador));
                    }
                }

                if (ImageData != null)
                {
                    var array = new Byte[ImageData.ContentLength];
                    ImageData.InputStream.Position = 0;
                    ImageData.InputStream.Read(array, 0, ImageData.ContentLength);
                    requerimiento.imagen = array;
                }
                else
                {
                    if (modelo.file == "")
                    {
                        requerimiento.imagen = null;
                    }
                    else
                    {
                        requerimiento.imagen = Encoding.ASCII.GetBytes(modelo.file);
                    }
                }
                //Se hace el split para separar los criterios de aceptación y meterlos en una lista
                var criterios = modelo.criteriosAceptacion.Split('|').ToList();
                //Se crea la lista de criterios de aceptacion que puede ser expandible
                var criterio_list = new List<Requerimientos_Cri_Acep>();
                foreach (var criterio in criterios)
                {
                    var cri_ac = new Requerimientos_Cri_Acep();
                    cri_ac.criterio = criterio;
                    criterio_list.Add(cri_ac);
                }
                requerimiento.Requerimientos_Cri_Acep = criterio_list;
                requerimiento.categoria = "Solicitud";
                baseDatos.Requerimientos.Add(requerimiento);    // Con esta linea se notifica a la base que se hacen los cambios
                var solicitud = new Solicitud_Cambios();
                solicitud.razon = modelo.razon;
                solicitud.req1 = requerimientoViejo.id;
                solicitud.req2 = requerimiento.id;
                solicitud.proyecto = requerimiento.proyecto;
                solicitud.solicitadoEn = DateTime.Now;
                solicitud.tipo = "Modificar";
                solicitud.estado = "En revisión";
                String userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
                solicitud.solicitadoPor = baseDatos.Usuarios.Where(m => m.id == userID).First().cedula;
                baseDatos.Solicitud_Cambios.Add(solicitud);
                baseDatos.SaveChanges();    // Se guardan los cambios en la base
                this.AddToastMessage("Solicitud de Cambio Creada", "La solicitud de modificar " + modelo.nombre + " se ha enviado correctamente.", ToastType.Success);      // Se muestra un mensaje de confirmacion
                return RedirectToAction("index", "Requerimientos", new { proyecto = requerimiento.proyecto });
            }

            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            string desarrolladorRol = context.Roles.Where(m => m.Name == "Desarrollador").First().Id;

            foreach (var user in context.Users.ToArray())                   // En esta seccion se cargan las listas que despliegan los
            {                                                               // desarrolladores y usuarios relacionados con el requerimiento
                if (user.Roles.First().RoleId.Equals(clienteRol))           // para modificarlos
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
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");     // Se hacen unas validaciones de permisos y se
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");                   // cargan los Viewbags necesitados en la vista
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");
            return View(modelo);    // Se retorna la vista al modelo luego de modificar los datos
        }

        List<Solicitud_Cambios> getRequerimientos(List<Solicitud_Cambios> solicitudes)
        {
            if (solicitudes != null && solicitudes.Count > 0)
            {
                var izquierda = new List<Solicitud_Cambios>();
                var derecha = new List<Solicitud_Cambios>();
                foreach (var sol in solicitudes)
                {
                    var izq = getRequerimientos(baseDatos.Solicitud_Cambios.Where(m => m.proyecto == sol.proyecto && m.estado == "Aprobado" && m.req2 == sol.req1).ToList());
                    izquierda.AddRange(izq);
                    var der = getRequerimientos(baseDatos.Solicitud_Cambios.Where(m => m.proyecto == sol.proyecto && m.estado == "Aprobado" && m.req1 == sol.req2).ToList());
                    derecha.AddRange(der);
                }
                var respuesta = solicitudes;
                respuesta.AddRange(izquierda);
                respuesta.AddRange(derecha);
                return respuesta;
            }
            else
            {
                return solicitudes;
            }
        }

        // POST: Crear Solicitud de Borrado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearSolicitudBorrado(CrearSolicitudModel modelo)
        {
            Requerimiento requerimiento;
            if (!string.IsNullOrWhiteSpace(modelo.razon2))
            {
                requerimiento = baseDatos.Requerimientos.Find(modelo.idReqAnterior);
                var solicitud = new Solicitud_Cambios();
                solicitud.razon = modelo.razon2;
                solicitud.req1 = requerimiento.id;
                solicitud.proyecto = requerimiento.proyecto;
                solicitud.solicitadoEn = DateTime.Now;
                solicitud.tipo = "Eliminar";
                solicitud.estado = "En revisión";
                String userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
                solicitud.solicitadoPor = baseDatos.Usuarios.Where(m => m.id == userID).First().cedula;
                baseDatos.Solicitud_Cambios.Add(solicitud);
                baseDatos.SaveChanges();    // Se guardan los cambios en la base
                this.AddToastMessage("Solicitud de Borrado Creada", "La solicitud de eliminar " + modelo.nombre + " se ha enviado correctamente.", ToastType.Success);      // Se muestra un mensaje de confirmacion
                return RedirectToAction("index", "Requerimientos", new { proyecto = requerimiento.proyecto });
            }
            this.AddToastMessage("Error al Crear la Solicitud de Borrado", "El campo de motivo es requerido.", ToastType.Error);      // Se muestra un mensaje de confirmacion
            return RedirectToAction("CrearSolicitud", new { id= modelo.idReqAnterior });
        }

        // GET: Crear Solicitud_Cambios
        public ActionResult Detalles(string id, int? page)
        {
            if (!revisarPermisos("Consultar Detalles de Requerimiento"))   // Revisa los permisos del usuario accediendo a la pantalla
            {
                //Despliega mensaje en caso de no poder modificar un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para ver detalles de requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            if (id == null) // Si no existe el requerimiento, redirecciona a error
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SolicitudCambiosModel modelo = new SolicitudCambiosModel();   // Crea un modelo y lo llena con los datos del requerimiento
            var solicitud_cambio = baseDatos.Solicitud_Cambios.Find(Int32.Parse(id));
            if (solicitud_cambio == null)
            {
                return HttpNotFound();
            }
            modelo.id = solicitud_cambio.id;
            modelo.proyecto = solicitud_cambio.proyecto;
            modelo.estadoSolicitud = solicitud_cambio.estado;
            modelo.tipoSolicitud = solicitud_cambio.tipo;
            modelo.razon = solicitud_cambio.razon;
            modelo.solicitadoPorSolicitud = solicitud_cambio.solicitadoPor;
            modelo.solicitadoEn = solicitud_cambio.solicitadoEn.ToString("MM/dd/yyyy");

            if (solicitud_cambio.aprobadoEn != null)
            {
                modelo.aprobadoEn = solicitud_cambio.aprobadoEn.Value.ToString("MM/dd/yyyy");
            }
            modelo.aprobadoPor = solicitud_cambio.aprobadoPor;
            modelo.idReqActual = solicitud_cambio.req1;
            modelo.idReqSolicitud = solicitud_cambio.req2;

            int? idReq;

            if (solicitud_cambio.tipo == "Modificar") {
                idReq = modelo.idReqSolicitud;
            } else {
                idReq = modelo.idReqActual;
            }

            var requerimiento = baseDatos.Requerimientos.Find(idReq);   // que entro como parametro
            if (requerimiento == null) {
                return HttpNotFound();
            }

            /*******************/
            modelo.codigo = requerimiento.codigo;
            modelo.nombre = requerimiento.nombre;
            modelo.descripcion = requerimiento.descripcion;
            modelo.solicitadoPor = requerimiento.solicitadoPor;
            modelo.creadoPor = requerimiento.creadoPor;
            modelo.prioridad = requerimiento.prioridad.ToString();
            modelo.esfuerzo = requerimiento.esfuerzo.ToString();
            modelo.fechaInicial = requerimiento.creadoEn.ToString("MM/dd/yyyy");
            if (requerimiento.finalizaEn != null) {
                modelo.fechaFinal = (requerimiento.finalizaEn ?? DateTime.Now).ToString("MM/dd/yyyy");
            }
            modelo.estado = requerimiento.estado;
            modelo.observaciones = requerimiento.observaciones;


            if (requerimiento.imagen != null) {
                modelo.file = HttpUtility.UrlEncode(Convert.ToBase64String(requerimiento.imagen));
            } else {
                modelo.file = "";
            }

            modelo.equipo = new List<string>();     // Se llena la variable equipo con el equipo ya asignado a este requerimiento, si ya hay uno
            foreach (var des in requerimiento.Usuarios.ToList()) {
                modelo.equipo.Add(des.cedula);
            }

            modelo.criteriosAceptacion = requerimiento.Requerimientos_Cri_Acep.Select(c => c.criterio).Aggregate((acc, x) => acc + "|" + x);    // Se agrega a la lista de criterios de aceptacion 
            List<Usuario> listaDesarrolladores = new List<Usuario>();       // Se inicializan listas que se usan a traves a continuacion
            List<Modulo> listaModulos = new List<Modulo>();
            List<Estado_Requerimientos> listaEstadoRequerimientos = new List<Estado_Requerimientos>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            //Requerido para formar el equipo de trabajo
            foreach (var proyEquipo in baseDatos.Proyectos.Find(requerimiento.proyecto).Proyecto_Equipo) {
                listaDesarrolladores.Add(baseDatos.Usuarios.Find(proyEquipo.usuario));
            }
            foreach (var user in context.Users.ToArray()) {
                if (user.Roles.First().RoleId.Equals(clienteRol)) {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                }
            }

            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");
            return View(modelo);        // Se retorna la vista al modelo luego de cargar los datos
        }

        // POST: Crear Solicitud de Cambio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(SolicitudCambiosModel modelo, HttpPostedFileBase ImageData)
        {
            if (ModelState.IsValid)     // Verifica si el modelo que entra como parametro es valido para modificar
            {

                var solicitud_cambio = baseDatos.Solicitud_Cambios.Find(modelo.id);
                if (solicitud_cambio == null) {
                    return HttpNotFound();
                }
                solicitud_cambio.id = modelo.id;
                solicitud_cambio.proyecto = modelo.proyecto;
                solicitud_cambio.estado = modelo.estado;
                solicitud_cambio.tipo = modelo.tipoSolicitud;
                solicitud_cambio.razon = modelo.razon;
                solicitud_cambio.solicitadoPor = modelo.solicitadoPor;
                solicitud_cambio.solicitadoEn = DateTime.ParseExact(modelo.solicitadoEn, "MM/dd/yyyy", null);
                if (solicitud_cambio.aprobadoEn != null) {
                    solicitud_cambio.aprobadoEn = DateTime.ParseExact(modelo.aprobadoEn, "MM/dd/yyyy", null);
                }

                solicitud_cambio.aprobadoPor = modelo.aprobadoPor;
                solicitud_cambio.req1 = modelo.idReqActual;
                solicitud_cambio.req2 = modelo.idReqSolicitud;

                var estado = baseDatos.Estado_Solicitud.Find(modelo.estadoSolicitud);
                solicitud_cambio.Estado_Solicitud = estado;
                var requerimiento = baseDatos.Requerimientos.Find(modelo.idReqSolicitud);   // Se busca el modelo en la base y se cambian sus datos por los
                requerimiento.nombre = modelo.nombre;                           // del modelo que entra como parametro
                requerimiento.codigo = modelo.codigo;
                requerimiento.creadoPor = modelo.creadoPor;
                requerimiento.descripcion = modelo.descripcion;
                requerimiento.solicitadoPor = modelo.solicitadoPor;
                requerimiento.prioridad = Int32.Parse(modelo.prioridad);
                requerimiento.esfuerzo = Int32.Parse(modelo.esfuerzo);
                requerimiento.creadoEn = DateTime.ParseExact(modelo.fechaInicial, "MM/dd/yyyy", null);
                if (modelo.fechaFinal != null) {
                    requerimiento.finalizaEn = DateTime.ParseExact(modelo.fechaFinal, "MM/dd/yyyy", null);
                }
                requerimiento.estado = modelo.estado;
                requerimiento.observaciones = modelo.observaciones;
                requerimiento.proyecto = modelo.proyecto;
                requerimiento.Usuarios = new List<Usuario>();
                if (modelo.equipo != null) {
                    foreach (var desarrollador in modelo.equipo) {
                        requerimiento.Usuarios.Add(baseDatos.Usuarios.Find(desarrollador));
                    }
                }

                if (ImageData != null) {
                    var array = new Byte[ImageData.ContentLength];
                    ImageData.InputStream.Position = 0;
                    ImageData.InputStream.Read(array, 0, ImageData.ContentLength);

                    requerimiento.imagen = array;
                } else {
                    if (modelo.file == "") {
                        requerimiento.imagen = null;
                    }
                }

                var reqcriacs = baseDatos.Requerimientos.Find(modelo.id).Requerimientos_Cri_Acep.ToList();

                foreach (var reqcriac in reqcriacs) {
                    baseDatos.Entry(reqcriac).State = System.Data.Entity.EntityState.Deleted;
                }
                //Se hace el split para separar los criterios de aceptación y meterlos en una lista
                var criterios = modelo.criteriosAceptacion.Split('|').ToList();

                //Se crea la lista de criterios de aceptacion que puede ser expandible
                var criterio_list = new List<Requerimientos_Cri_Acep>();
                foreach (var criterio in criterios) {
                    var cri_ac = new Requerimientos_Cri_Acep();
                    cri_ac.criterio = criterio;
                    criterio_list.Add(cri_ac);
                }

                requerimiento.Requerimientos_Cri_Acep = criterio_list;


                baseDatos.Entry(solicitud_cambio).State = System.Data.Entity.EntityState.Modified;     // Con esta linea se notifica a la base que se hacen los cambios
                baseDatos.Entry(requerimiento).State = System.Data.Entity.EntityState.Modified;     // Con esta linea se notifica a la base que se hacen los cambios
                baseDatos.SaveChanges();    // Se guardan los cambios en la base

                var sprints = baseDatos.Requerimientos.Find(modelo.idReqActual).Modulo1.Sprint_Modulos.Select(x => x.sprint);
                foreach (var sprint in sprints) {
                    updateSprintPoints(modelo.proyecto, sprint);
                }
                this.AddToastMessage("Requerimiento Modificado", "La solicitud de cambio del requerimiento " + modelo.nombre + " se ha modificado correctamente.", ToastType.Success);      // Se muestra un mensaje de confirmacion
                return RedirectToAction("Detalles", "Solicitud_Cambios", new { id = solicitud_cambio.id });       // Se carga el requerimiento modificado en la pantalla

            }

            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            string desarrolladorRol = context.Roles.Where(m => m.Name == "Desarrollador").First().Id;

            foreach (var user in context.Users.ToArray())                   // En esta seccion se cargan las listas que despliegan los
            {                                                               // desarrolladores y usuarios relacionados con el requerimiento
                if (user.Roles.First().RoleId.Equals(clienteRol))           // para modificarlos
                {
                    listaClientes.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                } else {
                    if (user.Roles.First().RoleId.Equals(desarrolladorRol))
                    {
                        listaDesarrolladores.Add(baseDatos.Usuarios.Where(m => m.id == user.Id).First());
                    }
                }
            }

            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");     // Se hacen unas validaciones de permisos y se
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");                   // cargan los Viewbags necesitados en la vista
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre"); ;

            return View(modelo);    // Se retorna la vista al modelo luego de modificar los datos
        }

        /// <summary>
        /// Funcionalidad de borrar requerimientos.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        /// <param name="model"> Modelo con la informacion del Usuario a borrar.</param>
        //POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Borrar(SolicitudCambiosModel modelo)
        {
            var solicitud = baseDatos.Solicitud_Cambios.Find(modelo.id);
            String proyecto = solicitud.proyecto;

            baseDatos.Entry(solicitud).State = System.Data.Entity.EntityState.Deleted;
            baseDatos.SaveChanges();

            this.AddToastMessage("Solicitud de Cambio Borrada", "La solicitud de cambio " + modelo.nombre + " se ha borrado correctamente.", ToastType.Success);
            return RedirectToAction("Index", "Solicitud_Cambios", new { proyecto = proyecto });
        }

        /// <summary>
        /// Cuenta la cantidad de puntos que están "en progreso"
        /// </summary>
        /// <param name="Proyecto"> El proyecto de donde se va a contar.</param>
        /// <param name="sprint"> El sprint de donde se va a contar.</param>
        private void updateSprintPoints(string Proyecto, int sprint)
        {

            var sprint_actual = baseDatos.Sprints.Find(Proyecto, sprint);
            // busca el dia actual o el dia donde comienza el sprint, el que sea el maximo
            var hoy = DateTime.Today < sprint_actual.fechaInicio ? sprint_actual.fechaInicio : DateTime.Today;

            //encontrar la tupla en la tabla Progreso_Sprint que cumple que el día es igual al maximo entre hoy y el primer día del sprint,
            // y esta en el proyecto y sprint pedido
            var progreso_hoy = baseDatos.Progreso_Sprint.Where(x => x.sprintProyecto == Proyecto && x.sprintNumero == sprint &&
            x.fecha.Year == hoy.Year
            && x.fecha.Month == hoy.Month
            && x.fecha.Day == hoy.Day
            ).FirstOrDefault();

            //sirve para revisar si se ha modificado la tupla
            var modificado = true;

            // si no hay un progreso_sprint creado para ese día, crearlo
            if (progreso_hoy == default(Progreso_Sprint)) {
                progreso_hoy = new Progreso_Sprint();
                progreso_hoy.fecha = hoy;
                progreso_hoy.sprintNumero = sprint;
                progreso_hoy.sprintProyecto = Proyecto;
                modificado = false;
            }

            // luego contar los puntos para ese progreso_sprint
            var puntos = sprint_actual.Sprint_Modulos
                .Select(m => baseDatos.Modulos
                    .Find(m.proyecto, m.modulo).Requerimientos
                        .Select(x => x.estado == "Finalizado" ? 0 : (x.esfuerzo ?? 0))
                        .Sum())
                .Sum();
            // asignar los puntos
            progreso_hoy.puntos = puntos;

            //actualizar si fue modificado, sino agregarlo
            if (modificado) {
                baseDatos.Entry(progreso_hoy).State = System.Data.Entity.EntityState.Modified;
            } else {
                baseDatos.Progreso_Sprint.Add(progreso_hoy);
            }
            baseDatos.SaveChanges();
        }
    }
}