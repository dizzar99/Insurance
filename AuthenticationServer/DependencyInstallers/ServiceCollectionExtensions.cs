using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace AuthenticationServer.DependencyInstallers
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDependencyFromAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            var installers = typeof(Startup).Assembly.ExportedTypes
                .Where(t => typeof(IDependencyInstaller).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(Activator.CreateInstance)
                .Cast<IDependencyInstaller>()
                .ToList();

            installers.ForEach(installer => installer.Install(services, configuration));
        }
    }
}
