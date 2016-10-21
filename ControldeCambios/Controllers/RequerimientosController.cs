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

namespace ControldeCambios.Controllers
{
    public class RequerimientosController : Controller
    {
        Entities baseDatos = new Entities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(RequerimientosModelo model)
        {
            if (ModelState.IsValid)
            {

            }

            return View(model);
        }

        public ActionResult Index(string id)
        {
            var proyecto = baseDatos.Proyectos.Find(id);
            var model = new RequerimientosIndexModel();

            model.titulo = proyecto.nombre;
            model.descripcion = proyecto.descripcion;
            model.cliente = proyecto.cliente;
            model.lider = proyecto.lider;

            var sprints = baseDatos.Sprints.Where(m => m.proyecto == id).ToList();

            var numeros_sprints = sprints.Select(m => m.numero);
            var sprint_modulos = baseDatos.Sprint_Modulo.Where(m => numeros_sprints.Contains(m.sprint) && m.proyecto == id).ToList();
            var sprint_modulo_requerimientos = sprint_modulos.Select(m => m.Requerimientos.ToList());

            if (sprint_modulo_requerimientos.Any())
            {
                model.reqs = sprint_modulo_requerimientos.Aggregate((acc, x) => acc.Concat(x).ToList());
            } else {
                model.reqs = new List<Requerimiento>();
            }



            return View(model);
        }
    }
}