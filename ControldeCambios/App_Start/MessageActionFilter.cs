using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControldeCambios.Controllers;
using System.Web.Mvc;

namespace ControldeCambios.App_Start
{
    public class MessagesActionFilter : ActionFilterAttribute
    {
        // This method is called BEFORE the action method is executed
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check for incoming Toastr objects, in case we've been redirected here
            ToastrController controller = filterContext.Controller as ToastrController;
            if (controller != null)
                controller.Toastr = (controller.TempData["Toastr"] as Toastr)
                                     ?? new Toastr();

            base.OnActionExecuting(filterContext);
        }

        // This method is called AFTER the action method is executed but BEFORE the
        // result is processed (in the view or in the redirection)
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ToastrController controller = filterContext.Controller as ToastrController;
            if (filterContext.Result.GetType() == typeof(ViewResult))
            {
                if (controller != null && controller.Toastr != null && controller.Toastr.ToastMessages != null && controller.Toastr.ToastMessages.Count() > 0)
                {
                    // We're going to a view so we store Toastr in the ViewData collection
                    controller.ViewData["Toastr"] = controller.Toastr;
                }
            }
            else if (filterContext.Result.GetType() == typeof(RedirectToRouteResult))
            {
                if (controller != null && controller.Toastr != null && controller.Toastr.ToastMessages.Count() > 0)
                {
                    // User is being redirected to another action method so we store Toastr in
                    // the TempData collection
                    controller.TempData["Toastr"] = controller.Toastr;
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}