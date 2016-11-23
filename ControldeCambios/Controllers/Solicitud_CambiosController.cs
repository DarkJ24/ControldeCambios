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
            var reqs = baseDatos.Requerimientos.Where(m => m.proyecto == proyecto && m.categoria == "Solicitud" && m.creadoPor == usuarioActual).ToList();
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
            var reqs = baseDatos.Requerimientos.Where(m => m.proyecto == proyecto && m.categoria == "Solicitud").ToList();
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
            //model.crearRequerimientos = revisarPermisos("Crear Requerimientos");
            //model.detallesRequerimientos = revisarPermisos("Consultar Detalles de Requerimiento");
            return View(model);
        }


        public ActionResult Aceptar(string id)
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
            AceptarSolicitudCambioModelo modelo = new AceptarSolicitudCambioModelo();   // Crea un modelo y lo llena con los datos del requerimiento
            var requerimiento = baseDatos.Requerimientos.Find(Int32.Parse(id));   // que entro como parametro
            if (requerimiento == null)
            {
                return HttpNotFound();
            }
            modelo.id = requerimiento.id;             // Diferentes asignaciones de variables para el modelo creado
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
            
            if(requerimiento.imagen != null)
            {
                modelo.file = HttpUtility.UrlEncode(Convert.ToBase64String(requerimiento.imagen));
            } else
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
            ViewBag.eliminarRequerimiento = revisarPermisos("Eliminar Requerimientos");              // Aqui se hacen unas validaciones de permisos 
            ViewBag.modificarRequerimiento = revisarPermisos("Modificar Requerimientos");            // y se cargan ciertos Viewbags necesitados por la vista
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Proyecto.ToList(), "nombre", "nombre");
            return View(modelo);        // Se retorna la vista al modelo luego de cargar los datos
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
    }
}