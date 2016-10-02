using ControldeCambios.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

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

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin rool   
                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.UserName = "admin";
                user.Email = "admin@example.com";

                string userPWD = "admin";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
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
