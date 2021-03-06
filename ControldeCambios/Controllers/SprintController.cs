﻿using ControldeCambios.App_Start;
using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
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
    public class SprintController : ToastrController
    {

        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

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
            if (progreso_hoy == default(Progreso_Sprint))
            {
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

        /// <summary>
        /// Funcionalidad para index Sprint.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        // GET: Sprint
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Funcionalidad para crear Sprint.
        /// </summary>
        /// <param name="proyecto"> Cadena con la nombre del Proyecto al que pertenece el sprint a  crear.</param>
        /// <returns>Modelo con la informacion del sprint</returns>
        // GET: Crear
        public ActionResult Crear(string proyecto)
        {
            if (!revisarPermisos("Crear Proyectos"))
            {
                //despliega mensaje en caso de no poder crear un usuario
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear sprints!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

            //se revisa que el proyecto exista
            if (String.IsNullOrEmpty(proyecto))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (baseDatos.Proyectos.Find(proyecto) != null)
            {
                var model = new SprintModelo();
                model.proyecto = proyecto;
                ViewBag.modulos = new MultiSelectList(baseDatos.Modulos.Where(m => m.proyecto == proyecto).ToList(), "numero", "nombre");
                return View(model);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Funcionalidad para crear Sprint.
        /// </summary>
        /// <param name="model">Modelo con la informacion del sprint a  crear.</param>
        /// <returns>Pagina de Crear Sprint</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(SprintModelo model)
        {
            if (ModelState.IsValid)
            {
                //se carga la informacion del sprint del modelo
                var sprint = new Sprint();
                sprint.numero = Int32.Parse(model.numero);
                sprint.proyecto = model.proyecto;
                sprint.fechaInicio = DateTime.ParseExact(model.fechaInicio, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                sprint.fechaFinal = DateTime.ParseExact(model.fechaFinal, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                if (sprint.fechaInicio > sprint.fechaFinal)
                {
                    this.AddToastMessage("Error", "La fecha de inicio debe ser antes de la fecha final", ToastType.Warning);
                    return RedirectToAction("Crear", "Sprint", new { proyecto = model.proyecto });
                }
                //se crea las relaciones de sprint con modulo
                if (model.modulos != null && model.modulos.Count() > 0)
                {
                    foreach (var modulo in model.modulos)
                    {
                        var sprint_modulo = new Sprint_Modulos();
                        sprint_modulo.proyecto = sprint.proyecto;
                        sprint_modulo.sprint = sprint.numero;
                        sprint_modulo.modulo = Int32.Parse(modulo);
                        sprint.Sprint_Modulos.Add(sprint_modulo);
                    }
                }

                baseDatos.Sprints.Add(sprint);
                baseDatos.SaveChanges();

                updateSprintPoints(sprint.proyecto, sprint.numero);
                this.AddToastMessage("Sprint Creado", "El sprint " + model.numero + " se ha creado y asignado correctamente"
                    + " al proyecto " + model.proyecto + ".", ToastType.Success);
                return RedirectToAction("Crear", "Sprint", new { proyecto = model.proyecto });

            }
            ViewBag.Proyectos = new SelectList(baseDatos.Proyectos.ToList(), "nombre", "nombre");
            ViewBag.modulos = baseDatos.Modulos.Where(m => m.proyecto == model.proyecto).ToList();
            return View(model);
        }
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


        /// <summary>
        /// Funcionalidad para consultar informacion del Sprint.
        /// </summary>
        /// <param name="proyecto"> Cadena con la nombre del Proyecto al que pertenece el sprint.</param>
        /// <param name="sprint"> Cadena con el numero de Sprint a consultar.</param>
        /// <returns>Modelo con la informacion del Sprint a consultar</returns>
        // GET: Informacion
        public ActionResult Informacion(string proyecto, string sprint)
        {
            if (!revisarPermisos("Consultar Detalles de Sprints"))
            {
                //despliega mensaje en caso de no poder crear un usuario
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar detalles de sprints!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            //se revisa que el proyecto y el sprint existan
            if (String.IsNullOrEmpty(proyecto))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (String.IsNullOrEmpty(sprint))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var proy = baseDatos.Proyectos.Find(proyecto);
            if (proy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var spr = baseDatos.Sprints.Find(proyecto, Int32.Parse(sprint));
            if (spr == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //se actualiza la informacion en el modelo
            var model = new SprintInfoModel();
            model.sprintNumero = sprint;
            model.sprintModulos = spr.Sprint_Modulos.Count();
            model.sprintFechaInicio = spr.fechaInicio.ToString("dd/MM/yyyy");
            model.sprintFechaFinal = spr.fechaFinal.ToString("dd/MM/yyyy");
            model.sprintEsfuerzo = 0;

            //esfuerzo obtenido de los requerimientos que pertenecen a los modulos asociados
            foreach (var sprMod in spr.Sprint_Modulos)
            {
                foreach (var req in sprMod.Modulo1.Requerimientos)
                {
                    model.sprintEsfuerzo += (req.esfuerzo ?? 0);
                }
            }

            //Charts
            DateTime start = spr.fechaInicio;
            DateTime end = spr.fechaFinal;
            ViewBag.dias = Enumerable.Range(0, 1 + end.Subtract(start).Days).Select(offset => start.AddDays(offset).ToString("dd/MM/yy")).ToArray();
            // extrapolar los valores que no se encuentran en la base de datos
            var horas_esfuerzo = baseDatos.Progreso_Sprint.Where(s => s.sprintNumero == spr.numero && s.sprintProyecto == proyecto).ToList();
            var esfuerzo_real = new List<double>();
            if (horas_esfuerzo.Any())
            {
                //tomar el ultimo dia para iterar por cada día
                var ultimo_dia = horas_esfuerzo.Select(h => h.fecha).Max();
                for (var dia = start; dia < ultimo_dia.AddDays(1); dia = dia.AddDays(1))
                {
                    double puntaje = 0;
                    var esfuerzo_actual = horas_esfuerzo.Where(s => s.fecha.Date.Equals(dia.Date)).FirstOrDefault();
                    // repetir el esfuerzo del día pasado si no se hizo nada
                    if (esfuerzo_actual == null)
                    {
                        if (esfuerzo_real.Count > 0) {
                            puntaje = esfuerzo_real.Last();
                        }
                    }
                    //sino, usar el puntaje normal
                    else
                    {
                        puntaje = esfuerzo_actual.puntos;
                    }
                    esfuerzo_real.Add(puntaje);
                }
            }
            ViewBag.esfuerzo_real = esfuerzo_real;
            var esfuerzo_total = baseDatos.Progreso_Sprint.Find(start, proyecto, spr.numero);
            // mostrar una estimacion del esfuerzo ideal si se encuentran datos acerca de los puntajes
            if (esfuerzo_total != null)
            {
                var puntos = esfuerzo_total.puntos;
                double longitud_del_sprint = end.Subtract(start).TotalDays;
                var velocidad = puntos / longitud_del_sprint;
                ViewBag.esfuerzo_ideal = Enumerable.Range(0, (int)longitud_del_sprint + 1).Select(x => System.Convert.ToDouble(((int)longitud_del_sprint - x) * velocidad)).ToList();
            }
            else
            {
                ViewBag.esfuerzo_ideal = new List<double>();
            }
            return View(model);
        }


        /// <summary>
        /// Funcionalidad para consultar informacion del Sprint.
        /// </summary>
        /// <param name="proyecto"> Cadena con la nombre del Proyecto al que pertenece el sprint.</param>
        /// <param name="sprint"> Cadena con el numero de Sprint a consultar.</param>
        /// <returns>Modelo con la informacion del Sprint a consultar</returns>
        // GET: Detalles
        public ActionResult Detalles(string proyecto, string sprint)
        {
            if (!revisarPermisos("Consultar Detalles de Sprints"))
            {
                //despliega mensaje en caso de no poder crear un usuario
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar detalles de sprints!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

            //se revisa que el proyecto y el sprint existan
            if (String.IsNullOrEmpty(proyecto))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (String.IsNullOrEmpty(sprint))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var proy = baseDatos.Proyectos.Find(proyecto);
            if (proy == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var spr = baseDatos.Sprints.Find(proyecto, Int32.Parse(sprint));
            if (spr == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new SprintModelo();

            //se actualiza la informacion dle modelo
            model.proyecto = proyecto;
            model.numero = sprint;
            model.fechaInicio = spr.fechaInicio.ToString("dd/MM/yyyy");
            model.fechaFinal = spr.fechaFinal.ToString("dd/MM/yyyy");

            //se carga la lista de modulos asociados
            int sprint_numero = Int32.Parse(sprint);
            ViewBag.listamodulos = new MultiSelectList(baseDatos.Modulos.Where(m => m.proyecto == proyecto).ToList(), "numero", "nombre");
            model.modulos = new List<string>();
            var listaDeModulosSprint = baseDatos.Sprint_Modulos.Where(m => m.sprint == sprint_numero && m.proyecto == proyecto).ToList();

            //se agregan los modulos al modelo
            foreach (var modulo in listaDeModulosSprint)
            {
                model.modulos.Add(modulo.modulo.ToString());
            }

            return View(model);
        }


        /// <summary>
        /// Funcionalidad para consultar informacion  basica del Sprint.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Sprint a consultar.</param>
        /// <returns>Pantalla de detalles del sprint</returns>
        // POST: /Sprint/Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(SprintModelo model)
        {
            if (ModelState.IsValid)
            {
                //se obtiene la informacion del modelo
                var sprint = new Sprint();
                sprint.numero = Int32.Parse(model.numero);
                sprint.proyecto = model.proyecto;
                sprint.fechaInicio = DateTime.ParseExact(model.fechaInicio, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                sprint.fechaFinal = DateTime.ParseExact(model.fechaFinal, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                baseDatos.Entry(sprint).State = System.Data.Entity.EntityState.Modified;

                //se eliminan los modulos viejos en caso de haber efectuado cambios
                var modulosViejos = baseDatos.Sprint_Modulos.Where(m => m.sprint == sprint.numero && m.proyecto == model.proyecto);
                if (modulosViejos.Count() > 0)
                {
                    foreach (var modulo in modulosViejos)
                    {

                        baseDatos.Entry(modulo).State = System.Data.Entity.EntityState.Deleted;
                    }
                }

                //se agregan las nuevas relaciones de sprint con modulo
                if (model.modulos != null && model.modulos.Count() > 0)
                {
                    foreach (var modulo in model.modulos)
                    {
                        var sprint_modulo = new Sprint_Modulos();
                        sprint_modulo.proyecto = sprint.proyecto;
                        sprint_modulo.sprint = sprint.numero;
                        sprint_modulo.modulo = Int32.Parse(modulo);
                        sprint.Sprint_Modulos.Add(sprint_modulo);
                        baseDatos.Entry(sprint_modulo).State = System.Data.Entity.EntityState.Added;
                        baseDatos.Entry(sprint).State = System.Data.Entity.EntityState.Modified;

                    }
                }

                baseDatos.SaveChanges();
                this.AddToastMessage("Sprint Modificado", "El sprint " + model.numero + " se ha modificado correctamente.", ToastType.Success);
                return RedirectToAction("Detalles", "Sprint", new { proyecto = model.proyecto, sprint = model.numero });
            }
            return View(model);
        }

        /// <summary>
        /// Funcionalidad para borrar un Sprint.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Sprint a borrar.</param>
        /// <returns>Pantalla de informacion del proyecto</returns>
        // POST: /Sprint/Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Borrar(SprintModelo model)
        {
            if (ModelState.IsValid)
            {
                int sprint_numero = Int32.Parse(model.numero);
                var modulosViejos = baseDatos.Sprint_Modulos.Where(m => m.sprint == sprint_numero && m.proyecto == model.proyecto).ToList();
                if (modulosViejos.Count() > 0)
                {
                    foreach (var modulo in modulosViejos)
                    {
                        baseDatos.Entry(modulo).State = System.Data.Entity.EntityState.Deleted;
                    }
                }
                
                var sprint = baseDatos.Sprints.Find(model.proyecto, sprint_numero);
                baseDatos.Entry(sprint).State = System.Data.Entity.EntityState.Deleted;
                baseDatos.SaveChanges();
                this.AddToastMessage("Sprint Eliminado", "El sprint " + model.numero + " se ha eliminado correctamente.", ToastType.Success);
                return RedirectToAction("Informacion", "Proyectos", new { id = model.proyecto });
            }
            return View(model);
        }
    }
}