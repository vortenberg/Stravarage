using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Stravarage.Startup))]
namespace Stravarage
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}
