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
        }
    }
}
