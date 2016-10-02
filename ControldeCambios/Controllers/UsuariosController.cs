using System.Linq;
using System.Web.Mvc;
using ControldeCambios.Models;
using System.Net;
using System;

namespace ControldeCambios.Controllers
{
    public class UsuariosController : Controller
    {
        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

        // GET: Usuarios
        public ActionResult Index()
        {
            UsuariosModelo modelo = new UsuariosModelo();
            modelo.roles = context.Roles.ToList();
            modelo.usuarios = baseDatos.Usuarios.ToList();
            modelo.identityUsuarios = context.Users.ToList();
            return View(modelo);
        }

        // GET: Detalles
        public ActionResult Detalles(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UsuariosModelo modelo = new UsuariosModelo();
            modelo.usuario = baseDatos.Usuarios.Find(id);
            if (modelo.usuario == null)
            {
                return HttpNotFound();
            }
            modelo.identityUsuario = context.Users.Find(modelo.usuario.id);
            if (modelo.identityUsuario == null)
            {
                return HttpNotFound();
            }
            modelo.telefonos = baseDatos.Usuarios_Telefonos.Where(a => a.usuario == modelo.usuario.cedula).ToList();
            if (modelo.telefonos != null && modelo.telefonos.Count > 0)
            {
                modelo.tel1 = modelo.telefonos.ElementAt(0);
            }
            if (modelo.telefonos.Count > 1)
            {
                modelo.tel2 = modelo.telefonos.ElementAt(1);
            }
            modelo.rol = context.Roles.Find(modelo.identityUsuario.Roles.First().RoleId);
            ViewBag.Name = new SelectList(context.Roles.ToList(), "Id", "Name", modelo.rol);
            return View(modelo);
        }
    }
}