using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControldeCambios.Controllers
{
    public class Solicitud_CambiosController : ToastrController
    {
        // GET: Solicitud_Cambios
        public ActionResult Index()
        {
            return View();
        }
    }
}