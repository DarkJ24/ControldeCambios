using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControldeCambios.App_Start;

namespace ControldeCambios.Controllers
{
    /// <summary>
    /// Provee funcionalidad para la ruta /Home/
    /// </summary>
    public class HomeController : ToastrController
    {
        /// <summary>
        /// Despliega la pagina index.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Despliega la pagina Acerca.
        /// </summary>
        /// <returns>Pagina de Acerca</returns>
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Despliega la pagina Contacto.
        /// </summary>
        /// <returns>Pagina de Contacto</returns>
        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}