using System.Linq;
using System.Web.Mvc;
using ControldeCambios.Models;

namespace ControldeCambios.Controllers
{

    public class Roles_PermisosController : Controller
    {
        ApplicationDbContext context;
        Entities baseDatos;

        public Roles_PermisosController()
        {
            context = new ApplicationDbContext();
            baseDatos = new Entities();
        }

        // GET: Roles_Permisos
        public ActionResult Index()
        {
            Roles_Permisos modelo = new Roles_Permisos();
            modelo.roles = context.Roles.ToList();
            modelo.permisos = baseDatos.Permisos.ToList();
            modelo.rol_permisos = baseDatos.Rol_Permisos.ToList();
            return View(modelo);
        }
    }
}