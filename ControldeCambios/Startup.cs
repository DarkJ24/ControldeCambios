using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(ControldeCambios.Startup))]
namespace ControldeCambios
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();
        }
        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();
            Entities baseDatos = new Entities();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin Role  
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                // Then we add all Permisos to Role (16 Permisos)
                for (int i = 1; i <= 24; i++)
                {
                    var rolPermisosEntry = new Rol_Permisos();
                    rolPermisosEntry.permiso = i;
                    rolPermisosEntry.rol = role.Id;
                    baseDatos.Rol_Permisos.Add(rolPermisosEntry);
                }

                //Here we create a Admin super user who will maintain the website   
                var user = new ApplicationUser();
                user.UserName = "admin@admin.com";
                user.Email = "admin@admin.com";
                string userPWD = "Admin.123";
                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");
                    //Create User
                    var userEntry = new Usuario();
                    userEntry.cedula = "000000000";
                    userEntry.nombre = "Administrador";
                    userEntry.id = user.Id;
                    userEntry.updatedAt = DateTime.Now;
                    baseDatos.Usuarios.Add(userEntry);
                }
                baseDatos.SaveChanges();
            }

            // creating Creating Desarrollador role    
            if (!roleManager.RoleExists("Desarrollador"))
            {
                var role = new IdentityRole();
                role.Name = "Desarrollador";
                roleManager.Create(role);
            }

            // creating Creating Cliente role    
            if (!roleManager.RoleExists("Cliente"))
            {
                var role = new IdentityRole();
                role.Name = "Cliente";
                roleManager.Create(role);
            }
        }
    }
}
