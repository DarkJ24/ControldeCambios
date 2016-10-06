using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControldeCambios.App_Start;

namespace ControldeCambios.Controllers
{
    public class HomeController : ToastrController
    {
        public ActionResult Index()
        {
            this.AddToastMessage("Bienvenido", "You made it all the way here!", ToastType.Success);
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}