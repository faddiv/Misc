using Database;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

namespace ViteCommerce.Api.Configurations;

public static class IdentityServerConfig
{
    public static void AddIdentityServerConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDefaultIdentity<IdentityUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
        })
            .AddEntityFrameworkStores<UserDbContext>();

        services.AddIdentityServer(options =>
        {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes
            options.EmitStaticAudienceClaim = true;
            options.UserInteraction.LoginUrl = "/Identity/Account/Login";
            options.UserInteraction.LoginReturnUrlParameter = "ReturnUrl";
        })
            .AddApiAuthorization<IdentityUser, UserDbContext>();

        services.AddControllersWithViews();
        services.AddRazorPages();
    }
}
