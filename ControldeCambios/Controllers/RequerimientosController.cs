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
            var reqs = baseDatos.Requerimientos.Where(m => m.proyecto == proyecto && m.categoria == "Actual").ToList();
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
            //Solo se permite crear cuando tiene un proyecto asignado
            if (String.IsNullOrEmpty(proyecto))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequerimientosModelo model = new RequerimientosModelo();
            model.proyecto = proyecto;
           //Requerido para cargar la lista de desarrolladores
            List<Usuario> listaDesarrolladores = new List<Usuario>();
            //Requerido para cargar la lista de clientes
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            //Requerido para formar el equipo de trabajo
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
        /// <param name="file"> Archivo de imagen subido</param>
        /// <param name="model"> Modelo con la informacion del Requerimiento a crear.</param>
        /// <returns>Pagina de Index</returns>
        // POST: /Requerimientos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(RequerimientosModelo model, HttpPostedFileBase ImageData)
        {



            if (ModelState.IsValid)
            {
                //Crea el requerimiento en la tabla Requerimientos
                var requerimiento = new Requerimiento();
                //Se llenan todos los atributos del requerimiento
                requerimiento.codigo = model.codigo;
                requerimiento.nombre = model.nombre;
                requerimiento.descripcion = model.descripcion;
                requerimiento.prioridad = Int32.Parse(model.prioridad);
                requerimiento.esfuerzo = Int32.Parse(model.esfuerzo);
                requerimiento.creadoEn = DateTime.ParseExact(model.fechaInicial, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                if (model.fechaFinal != null)
                {
                    requerimiento.finalizaEn = DateTime.ParseExact(model.fechaFinal, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                }
                requerimiento.observaciones = model.observaciones;
                requerimiento.solicitadoPor = model.solicitadoPor;
                requerimiento.creadoPor = model.creadoPor;
                requerimiento.estado = model.estado;
                requerimiento.proyecto = model.proyecto;
                requerimiento.version = 1;
                requerimiento.categoria = "Actual";
                requerimiento.Usuarios = model.equipo.Select(x => baseDatos.Usuarios.Find(x)).ToList();

                if (ImageData != null)
                {
                    var array = new Byte[ImageData.ContentLength];
                    ImageData.InputStream.Position = 0;
                    ImageData.InputStream.Read(array, 0, ImageData.ContentLength);

                    requerimiento.imagen = array;
                } else
                {
                    requerimiento.imagen = null;
                }

                //Se hace el split para separar los criterios de aceptación y meterlos en una lista
                var criterios = model.criteriosAceptacion.Split('|').ToList();

               //Se crea la lista de criterios de aceptacion que puede ser expandible
                var criterio_list = new List<Requerimientos_Cri_Acep>();
                foreach(var criterio in criterios)
                {
                    var cri_ac = new Requerimientos_Cri_Acep();
                    cri_ac.criterio = criterio;
                    criterio_list.Add(cri_ac);
                }

                requerimiento.Requerimientos_Cri_Acep = criterio_list;
                baseDatos.Requerimientos.Add(requerimiento);

               //Se guardan los datos en la base de datos
                baseDatos.SaveChanges();
               //Se muestra el mensaje de que el requerimiento fue creado correctamente
                this.AddToastMessage("Requerimiento Creado", "El requerimiento " + model.nombre + " se ha creado correctamente.", ToastType.Success);
               //Se devuelve a la pagina de crear para seguir creando requerimientos 
                return RedirectToAction("Crear", "Requerimientos", new { proyecto = model.proyecto });
            }
            List<Usuario> listaDesarrolladores = new List<Usuario>();
            List<Usuario> listaClientes = new List<Usuario>();
            string clienteRol = context.Roles.Where(m => m.Name == "Cliente").First().Id;
            //Requerido para formar el equipo de trabajo
            foreach (var proyEquipo in baseDatos.Proyectos.Find(model.proyecto).Proyecto_Equipo)
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
            RequerimientosModelo modelo = new RequerimientosModelo();   // Crea un modelo y lo llena con los datos del requerimiento
            modelo.requerimiento = baseDatos.Requerimientos.Find(id);   // que entro como parametro
            if (modelo.requerimiento.id == null)
            {
                return HttpNotFound();
            }
            modelo.id = modelo.requerimiento.id;             // Diferentes asignaciones de variables para el modelo creado
            modelo.codigo = modelo.requerimiento.codigo;
            modelo.nombre = modelo.requerimiento.nombre;
            modelo.creadoPor = modelo.requerimiento.creadoPor;
            modelo.version = modelo.requerimiento.version.ToString();
            modelo.descripcion = modelo.requerimiento.descripcion;
            modelo.prioridad = modelo.requerimiento.prioridad.ToString();
            modelo.esfuerzo = modelo.requerimiento.esfuerzo.ToString();
            modelo.observaciones = modelo.requerimiento.observaciones;
            modelo.fechaInicial = modelo.requerimiento.creadoEn.ToString("MM/dd/yyyy");
            if (modelo.requerimiento.finalizaEn != null)
            {
                modelo.fechaFinal = (modelo.requerimiento.finalizaEn ?? DateTime.Now).ToString("MM/dd/yyyy");
            }
            modelo.solicitadoPor = modelo.requerimiento.solicitadoPor;
            modelo.estado = modelo.requerimiento.estado;
            modelo.proyecto = modelo.requerimiento.proyecto;
            var requerimiento = baseDatos.Requerimientos.Find(id);  // Se crea una variable requerimiento con el id del requerimiento llamado
            modelo.requerimiento = requerimiento;
            
            if(modelo.requerimiento.imagen != null)
            {
                modelo.file = HttpUtility.UrlEncode(Convert.ToBase64String(modelo.requerimiento.imagen));
            } else
            {
                modelo.file = "";
            }

            modelo.equipo = new List<string>();     // Se llena la variable equipo con el equipo ya asignado a este requerimiento, si ya hay uno
            foreach (var des in modelo.requerimiento.Usuarios.ToList())
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
            foreach (var proyEquipo in baseDatos.Proyectos.Find(modelo.requerimiento.proyecto).Proyecto_Equipo)
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
            modelo.modificarRequerimiento = revisarPermisos("Modificar Requerimientos");            // y se cargan ciertos Viewbags necesitados por la vista
            ViewBag.Desarrolladores = new SelectList(listaDesarrolladores, "cedula", "nombre");
            ViewBag.Clientes = new SelectList(listaClientes, "cedula", "nombre");
            ViewBag.DesarrolladoresDisp = listaDesarrolladores;
            ViewBag.Estados = new SelectList(baseDatos.Estado_Requerimientos.ToList(), "nombre", "nombre");
            return View(modelo);        // Se retorna la vista al modelo luego de cargar los datos
        }

        /// <summary>
        /// Metodo encargado de la modificacion de requerimientos.
        /// </summary>
        /// <param name="id"> Modelo con la informacion del requerimiento para modificar.</param>
        /// <returns>Pagina de Detalles</returns>
        // POST: Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(RequerimientosModelo modelo, HttpPostedFileBase ImageData)
        {
            if (ModelState.IsValid)     // Verifica si el modelo que entra como parametro es valido para modificar
            {
                var requerimiento = baseDatos.Requerimientos.Find(modelo.id);   // Se busca el modelo en la base y se cambian sus datos por los
                requerimiento.nombre = modelo.nombre;                           // del modelo que entra como parametro
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
                }

                var reqcriacs = baseDatos.Requerimientos.Find(modelo.id).Requerimientos_Cri_Acep.ToList();

                foreach (var reqcriac in reqcriacs)
                {
                    baseDatos.Entry(reqcriac).State = System.Data.Entity.EntityState.Deleted;
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

                baseDatos.Entry(requerimiento).State = System.Data.Entity.EntityState.Modified;     // Con esta linea se notifica a la base que se hacen los cambios
                baseDatos.SaveChanges();    // Se guardan los cambios en la base


                if (requerimiento.Modulo1 != null)
                {

                    var sprint_modulo = baseDatos.Requerimientos.Find(modelo.id).Modulo1.Sprint_Modulos;

                    if (sprint_modulo != null)
                    {
                        var sprints = sprint_modulo.Select(x => x.sprint);
                        if (sprints != null)
                        {
                            foreach (var sprint in sprints)
                            {
                                updateSprintPoints(modelo.proyecto, sprint);
                            }
                        }

                    }

                }


                /*var sprints = baseDatos.Requerimientos.Find(modelo.id).Modulo1.Sprint_Modulos.Select(x => x.sprint);
                foreach(var sprint in sprints)
                {
                    updateSprintPoints(modelo.proyecto, sprint);
                }*/
                this.AddToastMessage("Requerimiento Modificado", "El requerimiento " + modelo.nombre + " se ha modificado correctamente.", ToastType.Success);      // Se muestra un mensaje de confirmacion
                return RedirectToAction("Detalles", "Requerimientos", new { id = requerimiento.id });       // Se carga el requerimiento modificado en la pantalla

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
        public ActionResult Borrar(RequerimientosModelo modelo)
        {
            var requerimiento = baseDatos.Requerimientos.Find(modelo.id);
            String proyecto = requerimiento.proyecto;
            var criterios = requerimiento.Requerimientos_Cri_Acep.ToList();
            for (int i = criterios.Count - 1; i >= 0; i--)
            {
                criterios.RemoveAt(i);
            }

            var equipo = requerimiento.Usuarios;

            foreach (var usuario in equipo.ToList())
            {
                requerimiento.Usuarios.Remove(usuario);
            }

            baseDatos.Entry(requerimiento).State = System.Data.Entity.EntityState.Deleted;
            baseDatos.SaveChanges();

            this.AddToastMessage("Requerimiento Borrado", "El requerimiento " + modelo.nombre + " se ha borrado correctamente.", ToastType.Success);
            return RedirectToAction("Index", "Requerimientos", new { proyecto = proyecto });
        }
    }
}      