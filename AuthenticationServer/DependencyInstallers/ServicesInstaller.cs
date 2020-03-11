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
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IMailSender, EmailSender>();
            services.AddScoped(s => MappingConfiguration.Init());
            services.AddSingleton(new EllipticCurve());
        }
    }
}
