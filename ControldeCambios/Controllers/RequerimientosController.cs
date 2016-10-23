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

            var sprint_modulos = baseDatos.Sprint_Modulo.Where(m => m.sprint == ssprint && m.proyecto == proyecto).ToList();
            var sprint_modulo_requerimientos = sprint_modulos.Select(m => m.Requerimientos.ToList());

            List<Requerimiento> reqs;

            if (sprint_modulo_requerimientos.Any())
            {
                reqs = sprint_modulo_requerimientos.Aggregate((acc, x) => acc.Concat(x).ToList());
            }
            else
            {
                reqs = new List<Requerimiento>();
            }

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
            var ultimo_dia = horas_esfuerzo.Select(h => h.fecha).Max();
            var esfuerzo_real = new List<double>();
            if (horas_esfuerzo.Any()) {

                for(var dia = start; dia < ultimo_dia.AddDays(1); dia = dia.AddDays(1))
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
        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Requerimientos"))
            {
                //despliega mensaje en caso de no poder crear un requerimiento
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear requerimientos!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Usuarios = baseDatos.Usuarios.ToList();
            return View();
        }


        /// <summary>
        /// Funcionalidad para crear Requerimientos.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Requerimiento a crear.</param>
        /// <returns>Pagina de Index</returns>
        // POST: /Requerimientos/Crear
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(RequerimientosModelo model)
        {
            if (ModelState.IsValid)
            {
                var requerimiento = new Requerimiento();

                requerimiento.codigo = model.codigo;
                requerimiento.nombre = model.nombre;
                requerimiento.descripcion = model.descripcion;
                requerimiento.solicitadoPor = model.solicitadoPor.ToString();


                baseDatos.Requerimientos.Add(requerimiento);
                baseDatos.SaveChanges();
            }

            return View(model);
        }*/
    }
}