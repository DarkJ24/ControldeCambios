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
        // GET: Requerimientos
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(RequerimientosModelo model)
        {
            if (ModelState.IsValid)
            {

            }

            return View(model);
        }
    }
}