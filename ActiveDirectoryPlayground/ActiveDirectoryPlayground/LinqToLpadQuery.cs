using LinqToLdap;
using LinqToLdap.Async;
using LinqToLdap.Mapping;
using System.DirectoryServices;
using System.Net;
using System.Security.Principal;

namespace ActiveDirectoryPlayground
{
    internal class LinqToLpadQuery
    {
        public static async Task ExecuteAsync()
        {

            var config = new LdapConfiguration()
                .MaxPageSizeIs(1000)
                .AddMapping(new LdapUserMapping());
            config.ConfigureFactory("FADDIV")
                .UsePort(50000)
                .AuthenticateBy(System.DirectoryServices.Protocols.AuthType.Negotiate)
                .AuthenticateAs(CredentialCache.DefaultNetworkCredentials)
                .ProtocolVersion(3);
            config.UseStaticStorage();

            using var conn = config.ConnectionFactory.GetConnection();
            conn.Bind();
            using var ctx = new DirectoryContext(conn);
            var result = await ctx.Query<LdapUser>()
                .Where(e => e.CommonName == "Abel_Harvey" || e.CommonName == "Adrienne_Schaefer")
                .Select(e => new
                {
                    e.DisplayName,
                    e.MemberOf
                })
                .ToListAsync();
            foreach (var item in result)
            {
                Console.WriteLine("{0} - {1}",
                    item.DisplayName,
                    string.Join("; ", item.MemberOf));
            }


        }
    }
    public class LdapUserMapping : ClassMap<LdapUser>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext("CN=Users,CN=faddiv,DC=localtest,DC=me");
            ObjectCategory("Person");
            ObjectClass("user");

            DistinguishedName(e => e.DistungishedName);
            Map(x => x.CommonName).Named("cn").ReadOnly();
            Map(x => x.ObjectGuid).Named("objectGUID").ReadOnly();
            Map(x => x.ObjectSid).Named("objectSid").ReadOnly();
            Map(x => x.DisplayName).Named("displayName");
            Map(x => x.Mail).Named("mail");
            Map(x => x.EmployeeId).Named("employeeID");
            Map(x => x.Country).Named("c");
            Map(x => x.MemberOf).Named("memberOf");

            return this;
        }
    }
    public class LdapUser
    {
        public string DistungishedName { get; set; }
        public string CommonName { get; set; }
        public string Mail { get; set; }
        public string EmployeeId { get; set; }
        public string DisplayName { get; set; }
        public string[] MemberOf { get; set; }
        public Guid ObjectGuid { get; set; }
        public SecurityIdentifier ObjectSid { get; set; }
        public string Country { get; set; }
    }
}
