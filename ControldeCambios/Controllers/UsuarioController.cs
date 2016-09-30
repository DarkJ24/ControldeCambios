using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControldeCambios.Models;

namespace ControldeCambios.Controllers
{
    public class UsuarioController : Controller
    {
        Entities baseDatos = new Entities();

        // GET: Usuario
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ModeloIntermedioIM modelo)
        {
            if (ModelState.IsValid)
            {
                baseDatos.Usuarios.Add(modelo.modeloUsuario);
                baseDatos.SaveChanges();

                if (modelo.modeloTelefono != null)
                {
                    modelo.modeloTelefono.telefono = modelo.modeloTelefono.telefono;
                    baseDatos.Usuarios_Telefonos.Add(modelo.modeloTelefono);
                }
                if (modelo.modeloTelefono2 != null)
                {
                    modelo.modeloTelefono2.telefono = modelo.modeloTelefono2.telefono;
                    baseDatos.Usuarios_Telefonos.Add(modelo.modeloTelefono2);
                }
                if (modelo.modeloTelefono3 != null)
                {
                    modelo.modeloTelefono3.telefono = modelo.modeloTelefono3.telefono;
                    baseDatos.Usuarios_Telefonos.Add(modelo.modeloTelefono3);
                }
                if (modelo.modeloTelefono4 != null)
                {
                    modelo.modeloTelefono4.telefono = modelo.modeloTelefono4.telefono;
                    baseDatos.Usuarios_Telefonos.Add(modelo.modeloTelefono4);
                }

                baseDatos.SaveChanges();
                return RedirectToAction("Index");

            }

            else
            {
                ModelState.AddModelError("", "Debe completar toda la información necesaria.");
                return View(modelo);
            }

        }
    }
}