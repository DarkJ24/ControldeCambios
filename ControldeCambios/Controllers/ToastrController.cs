using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControldeCambios.App_Start;

namespace ControldeCambios.Controllers
{
    public abstract class ToastrController : Controller
    {
        public ToastrController()
        {
            Toastr = new Toastr();
        }
        public Toastr Toastr { get; set; }

        public ToastMessage AddToastMessage(string title, string message, ToastType toastType)
        {
            return Toastr.AddToastMessage(title, message, toastType);
        }
    }
}