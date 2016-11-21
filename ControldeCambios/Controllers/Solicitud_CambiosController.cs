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