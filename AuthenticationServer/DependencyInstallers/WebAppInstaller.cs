using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationServer.DependencyInstallers
{
    public class WebAppInstaller : IDependencyInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging();
            services.AddControllers();
        }
    }
}
