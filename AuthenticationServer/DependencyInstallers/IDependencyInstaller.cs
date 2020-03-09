using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationServer.DependencyInstallers
{
    interface IDependencyInstaller
    {
        void Install(IServiceCollection services, IConfiguration configuration);
    }
}
