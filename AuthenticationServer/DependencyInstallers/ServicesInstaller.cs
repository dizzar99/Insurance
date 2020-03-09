using Insurance.BLL.Implementations;
using Insurance.BLL.Interface.Interfaces;
using Insurance.BLL.Mapper;
using Insurance.Common.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationServer.DependencyInstallers
{
    public class ServicesInstaller : IDependencyInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IServerKeyManager, DefaultKeyManager>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddSingleton(new EllipticCurve());
            services.AddScoped(s => MappingConfiguration.Init());
        }
    }
}
