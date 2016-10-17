using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControldeCambios.App_Start;

namespace ControldeCambios.Controllers
{
    /// <summary>
    /// Provee funcionalidad mostrar mensajes en pantalla cuando el usuario
    /// realiza acciones o hay algun cambio en la aplicacion.
    /// </summary>
    public abstract class ToastrController : Controller
    {
        public ToastrController()
        {
            Toastr = new Toastr();
        }
        public Toastr Toastr { get; set; }

        /// <summary>
        /// Metodo usado para desplegar un mensaje.
        /// </summary>
        /// <param name="title"> Titulo del mensaje.</param>
        /// <param name="message"> Cuerpo del mensaje.</param>
        /// <param name="toastType"> Tipo del objeto toastr.</param>
        /// <returns>Mensaje en pantalla</returns>
        public ToastMessage AddToastMessage(string title, string message, ToastType toastType)
        {
            return Toastr.AddToastMessage(title, message, toastType);
        }
    }
}