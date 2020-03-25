using Insurance.BLL.Interface.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace AuthenticationServer.DependencyInstallers
{
    public class WebAppInstaller : IDependencyInstaller
    {
        public void Install(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.Bind(nameof(JwtSettings), jwtSettings);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = this.GetKey(jwtSettings.Secret),
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = false,
                RequireExpirationTime = true,
                SaveSigninToken = false,
                ClockSkew = TimeSpan.Zero,
            };

            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = tokenValidationParameters;
                x.SaveToken = false;
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSingleton(jwtSettings);
            services.AddLogging();
            services.AddControllers();
        }

        private SymmetricSecurityKey GetKey(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

        private bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires != null)
            {
                return DateTime.UtcNow < expires;
            }
            return false;
        }

        class ReplayCache : ITokenReplayCache
        {
            public bool TryAdd(string securityToken, DateTime expiresOn)
            {
                return false;
            }

            public bool TryFind(string securityToken)
            {
                return false;
            }
        }
    }
}
