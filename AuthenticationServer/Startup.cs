using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth.BLL.Implementations;
using Auth.BLL.Interface.Interfaces;
using Auth.DataAccess;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Auth.BLL.Mapper;
using Auth.Common.Interface;

namespace AuthenticationServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ApplicationContext>();
            services.AddScoped<IServerKeyManager, DefaultKeyManager>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddSingleton(new EllipticCurve());
            services.AddScoped(s => MappingConfiguration.Init());
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
