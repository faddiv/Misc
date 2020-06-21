using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WinAuth.Data;

namespace WinAuth
{
    internal class AdAuthHandler : AuthenticationHandler<AdAuthOptions>
    {
        public AdAuthHandler(IOptionsMonitor<AdAuthOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock,
            Entities ctx) : base(options, logger, encoder, clock)
        {
            Ctx = ctx;
        }

        public Entities Ctx { get; }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(base.Context.User?.Identity.IsAuthenticated ?? false)
            {
                var identity = new ClaimsIdentity("AD", ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.Name, Context.User?.Identity.Name));
                var principal = new System.Security.Claims.ClaimsPrincipal(identity);
                var user = Ctx.Users.FirstOrDefault(e => e.UserName == Context.User.Identity.Name);
                if(user != null)
                {
                    string roleType = ((ClaimsIdentity)principal.Identity).RoleClaimType;
                    ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(roleType, "Admin"));
                }
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
            }
            return Task.FromResult(AuthenticateResult.Fail("No auth"));
        }
    }
}