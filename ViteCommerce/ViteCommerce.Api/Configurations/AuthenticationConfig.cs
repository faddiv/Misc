using Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace ViteCommerce.Api.Configurations
{
    public static class AuthenticationConfig
    {
        public static void AddAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication(opt =>
                {
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.RequireHttpsMetadata = false;
                    opt.TokenValidationParameters.ValidAudience =
                        configuration.GetValue<string>("Authentication:BFF:Audience") ?? "localhost";
                    opt.TokenValidationParameters.ValidIssuer = "http://localhost:3000";
                    var ssk = Utilities.GetKeyFromConfig(
                        configuration.GetValue<string>("Authentication:BFF:Secret") ?? "Default123");
                    opt.TokenValidationParameters.TokenDecryptionKey = ssk;
                    opt.TokenValidationParameters.IssuerSigningKey = ssk;
                    opt.TokenValidationParameters.ValidateAudience = true;
                    opt.TokenValidationParameters.ValidateIssuer = true;
                    opt.TokenValidationParameters.ValidateLifetime = true;
                    opt.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    opt.Events ??= new JwtBearerEvents();
                    opt.Events.OnMessageReceived = ctx =>
                    {
                        return Task.CompletedTask;
                    };
                })
                ;


        }
    }
}
