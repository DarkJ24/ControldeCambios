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
        IndexEntities baseDatos = new IndexEntities();
        // GET: Usuario

        public ActionResult Index()
        {
            ModeloIntermedioIndex modelo = new ModeloIntermedioIndex();
            modelo.listaUsuarios = baseDatos.Usuarios.ToList();
            modelo.listaAspUser = baseDatos.AspNetUsers.ToList();
            modelo.listaAspRoles = baseDatos.AspNetRoles.ToList();
            return View(modelo);
        }

    }
}