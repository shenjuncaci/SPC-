using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SPCSystem.Startup))]
namespace SPCSystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
