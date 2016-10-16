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
    public class UsuariosController : ToastrController
    {
        Entities baseDatos = new Entities();
        ApplicationDbContext context = new ApplicationDbContext();

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private bool revisarPermisos(string permiso)
        {
            String userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var rol = context.Users.Find(userID).Roles.First();
            var permisoID = baseDatos.Permisos.Where(m => m.nombre == permiso).First().codigo;
            var listaRoles = baseDatos.Rol_Permisos.Where(m => m.permiso == permisoID).ToList().Select(n => n.rol);
            bool userRol = listaRoles.Contains(rol.RoleId);

            return userRol;
        }

        // GET: Usuarios
        public ActionResult Index(int ? page)
        {
            if (!revisarPermisos("Consular Lista de Usuarios"))
            {
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar Usuarios!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            UsuariosModelo modelo = new UsuariosModelo();
            modelo.roles = context.Roles.ToList();
            modelo.usuarios = baseDatos.Usuarios.OrderByDescending(s => s.updatedAt).ToList();
            modelo.identityUsuarios = context.Users.ToList();
            modelo.indexUserInfoList = new List<UsuariosModelo.userInfo>();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (modelo.usuarios.Count < pageSize * pageNumber) ? modelo.usuarios.Count : pageSize * pageNumber;
            for (int i = (pageNumber-1)*pageSize; i < lastElement; i++)
            {
                UsuariosModelo.userInfo user = new UsuariosModelo.userInfo();
                user.cedula = modelo.usuarios.ElementAt(i).cedula;
                user.nombre = modelo.usuarios.ElementAt(i).nombre;
                user.identityId = modelo.usuarios.ElementAt(i).id;
                for (int j = 0; j < modelo.identityUsuarios.Count; j++)
                {
                    if (modelo.usuarios.ElementAt(i).id.Equals(modelo.identityUsuarios.ElementAt(j).Id))
                    {
                        user.correo = modelo.identityUsuarios.ElementAt(j).Email;
                        for (int k = 0; k < modelo.roles.Count; k++)
                        {
                            if (modelo.roles.ElementAt(k).Id.Equals(modelo.identityUsuarios.ElementAt(j).Roles.First().RoleId))
                            {
                                user.rol = modelo.roles.ElementAt(k).Name;
                            }
                        }
                    }
                }
                modelo.indexUserInfoList.Add(user);
            }
            modelo.crearUsuario = revisarPermisos("Crear Usuarios");
            modelo.detallesUsuario = revisarPermisos("Consultar Detalles de Usuarios");
            var usersAsIPagedList = new StaticPagedList<UsuariosModelo.userInfo>(modelo.indexUserInfoList, pageNumber, pageSize, modelo.usuarios.Count);
            ViewBag.OnePageOfUsers = usersAsIPagedList;
            return View(modelo);
        }

        // GET: Detalles
        public ActionResult Detalles(string id)
        {

            if(!revisarPermisos("Consultar Detalles de Usuarios"))
            {
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para ver detalles de usuarios!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }

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
            modelo.nombreUsuario = modelo.usuario.nombre;
            modelo.identityUsuario = context.Users.Find(modelo.usuario.id);
            if (modelo.identityUsuario == null)
            {
                return HttpNotFound();
            }
            modelo.email = modelo.identityUsuario.Email;
            modelo.telefonos = baseDatos.Usuarios_Telefonos.Where(a => a.usuario == modelo.usuario.cedula).ToList();
            if (modelo.telefonos != null && modelo.telefonos.Count > 0)
            {
                modelo.tel1 = modelo.telefonos.ElementAt(0).telefono;
            }
            if (modelo.telefonos.Count > 1)
            {
                modelo.tel2 = modelo.telefonos.ElementAt(1).telefono;
            }
            if (modelo.telefonos.Count > 2)
            {
                modelo.tel3 = modelo.telefonos.ElementAt(2).telefono;
            }
            String currentUser = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if (modelo.identityUsuario.Id != currentUser)
            {
                modelo.eliminarUsuario = revisarPermisos("Eliminar Usuarios");
            } else
            {
                modelo.eliminarUsuario = false;
            }
            modelo.modificarUsuario = revisarPermisos("Modificar Usuarios");

            modelo.rol = context.Roles.Find(modelo.identityUsuario.Roles.First().RoleId);
            ViewBag.Name = new SelectList(context.Roles.ToList(), "Name", "Name", modelo.rol);
            return View(modelo);
        }

        //POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Borrar(UsuariosModelo model)
        {
            var user = await baseDatos.Usuarios.FindAsync(model.usuario.cedula);
            baseDatos.Entry(user).State = System.Data.Entity.EntityState.Deleted;
            baseDatos.SaveChanges();

            var aspUser = await UserManager.FindByIdAsync(model.identityUsuario.Id);
            await UserManager.DeleteAsync(aspUser);

            this.AddToastMessage("Usuario Borrado", "El usuario "+ model.usuario.nombre +" se ha borrado correctamente.", ToastType.Success);
            return RedirectToAction("Index", "Usuarios");
        }

        // POST: Detalles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Detalles(UsuariosModelo model)
        {
            if (ModelState.IsValid)
            {
                var telefonos_viejos = baseDatos.Usuarios_Telefonos.Where(m => m.usuario == model.usuario.cedula);

                foreach (Usuarios_Telefonos telefono in telefonos_viejos)
                {
                    baseDatos.Entry(telefono).State = System.Data.Entity.EntityState.Deleted;
                }
                baseDatos.SaveChanges();

                var telefonoEntry = new Usuarios_Telefonos();
                telefonoEntry.telefono = model.tel1;
                telefonoEntry.usuario = model.usuario.cedula;

                baseDatos.Usuarios_Telefonos.Add(telefonoEntry);

                if (model.tel2 != null)
                {
                    var telefonoEntry2 = new Usuarios_Telefonos();
                    telefonoEntry2.telefono = model.tel2;
                    telefonoEntry2.usuario = model.usuario.cedula;
                    baseDatos.Usuarios_Telefonos.Add(telefonoEntry2);
                }

                if (model.tel3 != null)
                {
                    var telefonoEntry3 = new Usuarios_Telefonos();
                    telefonoEntry3.telefono = model.tel3;
                    telefonoEntry3.usuario = model.usuario.cedula;
                    baseDatos.Usuarios_Telefonos.Add(telefonoEntry3);
                }

                baseDatos.SaveChanges();

                var usuario = baseDatos.Usuarios.Find(model.usuario.cedula);
                usuario.nombre = model.nombreUsuario;
                usuario.updatedAt = DateTime.Now;

                baseDatos.Entry(usuario).State = System.Data.Entity.EntityState.Modified;
                baseDatos.SaveChanges();

                var aspUser = UserManager.FindById(model.identityUsuario.Id);
                aspUser.UserName = model.email;
                aspUser.Email = model.email;

                UserManager.Update(aspUser);

                var rolViejo = aspUser.Roles.SingleOrDefault().RoleId;
                var nombreRolViejo = context.Roles.SingleOrDefault(m => m.Id == rolViejo).Name;
                UserManager.RemoveFromRole(model.identityUsuario.Id, nombreRolViejo);
                UserManager.AddToRole(model.identityUsuario.Id, model.rol.Name);

                this.AddToastMessage("Usuario Modificado", "El usuario " + model.usuario.nombre + " se ha actualizado correctamente.", ToastType.Success);
                return RedirectToAction("Index", "Usuarios");

            }

            return View(model);
        }



        // GET: /Usuarios/Crear
        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Usuarios"))
            {
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear usuarios!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Name = new SelectList(context.Roles.ToList(), "Name", "Name");
            return View();
        }

        // POST: /Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Crear(CrearUsuarioModel model)
        {

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                try
                {
                    
                    string generatedPassword = Membership.GeneratePassword(12, 2);
                    var result = await UserManager.CreateAsync(user, generatedPassword);

                    if (result.Succeeded)
                    {

                        var userEntry = new Usuario();
                        userEntry.cedula = model.Cedula;
                        userEntry.nombre = model.Nombre;
                        userEntry.id = context.Users.Where(u => u.Email == model.Email).FirstOrDefault().Id;
                        userEntry.updatedAt = DateTime.Now;

                        baseDatos.Usuarios.Add(userEntry);
                        baseDatos.SaveChanges();

                        var telefonoEntry = new Usuarios_Telefonos();
                        telefonoEntry.telefono = model.Telefono;
                        telefonoEntry.usuario = model.Cedula;

                        baseDatos.Usuarios_Telefonos.Add(telefonoEntry);

                        if (model.Telefono2 != null)
                        {
                            var telefonoEntry2 = new Usuarios_Telefonos();
                            telefonoEntry2.telefono = model.Telefono2;
                            telefonoEntry2.usuario = model.Cedula;
                            baseDatos.Usuarios_Telefonos.Add(telefonoEntry2);
                        }

                        if (model.Telefono3 != null)
                        {
                            var telefonoEntry3 = new Usuarios_Telefonos();
                            telefonoEntry3.telefono = model.Telefono3;
                            telefonoEntry3.usuario = model.Cedula;
                            baseDatos.Usuarios_Telefonos.Add(telefonoEntry3);
                        }

                        baseDatos.SaveChanges();

                        string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account", model.Nombre, generatedPassword);

                        await this.UserManager.AddToRoleAsync(user.Id, model.UserRoles);
                        this.AddToastMessage("Usuario Creado", "El usuario " + model.Nombre + " se ha creado correctamente. Se envió un correo electronico de confirmación al usuario", ToastType.Success);
                        return RedirectToAction("Crear", "Usuarios");


                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    await UserManager.DeleteAsync(user);
                    ViewBag.Name = new SelectList(context.Roles.ToList(), "Name", "Name");
                    // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario

                    this.AddToastMessage("Error", "Ha ocurrido un error al crear al usuario " + model.Nombre + ".", ToastType.Error);
                    return View(model);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    this.AddToastMessage("Error", "Ha ocurrido un error al crear al usuario " + model.Nombre + ".", ToastType.Error);
                    ViewBag.Name = new SelectList(context.Roles.ToList(), "Name", "Name");
                    return View(model);
                }               
                              
            }
            return View(model);

            
        }

        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject, string usrName , string userPassword)
        {
            
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userID, subject, "Hola. <br><br>"
                + "Se ha creado el usuario "+ usrName + " en nuestro Sistema de Control de Cambios.<br>"
                + "Su contraseña provisional es: \'" + userPassword + "\'"
                + "<br><b>Por favor cambia tu contraseña.</b><br>"
                + "<br>Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

            return callbackUrl;
        }


    }
}