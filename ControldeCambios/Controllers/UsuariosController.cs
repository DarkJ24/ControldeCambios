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
    /// <summary>
    /// Provee funcionalidad para la ruta /Usuarios/
    /// </summary>
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

        /// <summary>
        /// Se utiliza para revisar que el rol del usuario que intenta acceder a alguna
        /// caracteristica tenga los permisos correspondientes.
        /// </summary>
        /// <param name="permiso"> Nombre del permiso que se intenta revisar.</param>
        /// <returns>Pagina de Index</returns>
        private bool revisarPermisos(string permiso)
        {
            String userID = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var rol = context.Users.Find(userID).Roles.First();
            var permisoID = baseDatos.Permisos.Where(m => m.nombre == permiso).First().codigo;
            var listaRoles = baseDatos.Rol_Permisos.Where(m => m.permiso == permisoID).ToList().Select(n => n.rol);
            bool userRol = listaRoles.Contains(rol.RoleId);

            return userRol;
        }

        /// <summary>
        /// Despliega la pagina index.
        /// </summary>
        /// <param name="page"> Parametro opcional que indica el numero de pagina para mostrar.</param>
        /// <returns>Pagina de Index</returns>
        // GET: Usuarios
        public ActionResult Index(int ? page)
        {
            if (!revisarPermisos("Consular Lista de Usuarios"))
            {
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para consultar Usuarios!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            //se obtiene la informacion del modelo de usuarios
            UsuariosModelo modelo = new UsuariosModelo();
            modelo.roles = context.Roles.ToList();
            modelo.usuarios = baseDatos.Usuarios.OrderByDescending(s => s.updatedAt).ToList();
            modelo.identityUsuarios = context.Users.ToList();
            modelo.indexUserInfoList = new List<UsuariosModelo.userInfo>();
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int lastElement = (modelo.usuarios.Count < pageSize * pageNumber) ? modelo.usuarios.Count : pageSize * pageNumber;
            
            //despliega la informacion de los usuarios por paginas
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

        /// <summary>
        /// Despliega la pagina Detalles.
        /// </summary>
        /// <param name="id"> Parametro que indica el id del usuario a consultar los detalles.</param>
        /// <returns>Pagina de Detalles</returns>
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

            //Valida que la informacion del modelo sea correcta
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

            // obtiene el usuario que esta autentificado para validar los permisos
            // segun su rol
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


        /// <summary>
        /// Funcionalidad de borrar Usuario.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        /// <param name="model"> Modelo con la informacion del Usuario a borrar.</param>
        //POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Borrar(UsuariosModelo model)
        {
            //obtiene el usuario a borrar de la base de datos
            var user = await baseDatos.Usuarios.FindAsync(model.usuario.cedula);
            baseDatos.Entry(user).State = System.Data.Entity.EntityState.Deleted;
            baseDatos.SaveChanges();

            var aspUser = await UserManager.FindByIdAsync(model.identityUsuario.Id);
            await UserManager.DeleteAsync(aspUser);

            this.AddToastMessage("Usuario Borrado", "El usuario "+ model.usuario.nombre +" se ha borrado correctamente.", ToastType.Success);
            return RedirectToAction("Index", "Usuarios");
        }

        /// <summary>
        /// Despliega la pagina Detalles.
        /// </summary>
        /// <param name="id"> Modelo con la informacion del Usuario a consultar los detalles.</param>
        /// <returns>Pagina de Detalles</returns>
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


        /// <summary>
        /// Funcionalidad para crear Usuario.
        /// </summary>
        /// <returns>Pagina de Index</returns>
        // GET: /Usuarios/Crear
        public ActionResult Crear()
        {
            if (!revisarPermisos("Crear Usuarios"))
            {
                //despliega mensaje en caso de no poder crear un usuario
                this.AddToastMessage("Acceso Denegado", "No tienes permiso para crear usuarios!", ToastType.Warning);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Name = new SelectList(context.Roles.ToList(), "Name", "Name");
            return View();
        }

        /// <summary>
        /// Funcionalidad para crear Usuario.
        /// </summary>
        /// <param name="model"> Modelo con la informacion del Usuario a crear.</param>
        /// <returns>Pagina de Index</returns>
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
                    
                    //generacion de contraseña provisional
                    string generatedPassword = Membership.GeneratePassword(12, 2);
                    //crea el usuario en la tabla de AspNetUsers
                    var result = await UserManager.CreateAsync(user, generatedPassword);

                    if (result.Succeeded)
                    {
                        // Crea el usuario en las tabla de Usuario
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

                        // valida los datos recibidos del modelo
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

                        //crea la relacion del usuario con el rol
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


        /// <summary>
        /// Metodo usado para enviar el email de confirmacion y activacion de cuenta
        /// para un usuario nuevo.
        /// </summary>
        /// <param name="userID"> Identificador del usuario.</param>
        /// <param name="subject"> Asunto del email.</param>
        /// <param name="usrName"> Nombre del usuario.</param>
        /// <param name="userPassword"> Contraseña provisional usada para la creacion de cuentas.</param>
        /// <returns>Pagina de Index</returns>
        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject, string usrName , string userPassword)
        {
            
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);

            // Construye y envia el mensaje de confirmacion
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