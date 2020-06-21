using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;

namespace WinAuth.Controllers
{
    public interface IActiveDirectoryService
    {
        List<KeyValuePair<string, string>> LoadData(string userName);
    }
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        public ActiveDirectoryService(IConfiguration configuration, ILogger<ActiveDirectoryService> log)
        {

            Config = configuration.GetSection("LdapConfig").Get<LdapConfig>();
            Log = log;
        }

        public LdapConfig Config { get; }
        public ILogger<ActiveDirectoryService> Log { get; }

        public List<KeyValuePair<string, string>> LoadData(string userName)
        {
            var list = new List<KeyValuePair<string, string>>();
            if (userName == null)
            {
                Log.LogInformation($"userName not provided.");
                return list;
            }
            Log.LogInformation($"Load {userName} from AD.");
            try
            {
                //using (var context = new PrincipalContext(ContextType.ApplicationDirectory, "localhost", "dc=maxcrc,dc=com")) // Use on full ActiveDirectory.
                using (var de = new DirectoryEntry(Config.Path, Config.UserName, Config.Password, Config.AuthenticationType))
                //using (var de = new DirectoryEntry("ldap.forumsys.com:389", "cn=gauss,ou=mathematicians,dc=example,dc=com", "password", AuthenticationTypes.Signing))
                {
                    using (var ds = new DirectorySearcher(de))
                    {
                        var name = userName.Substring(userName.IndexOf('\\') + 1);
                        ds.Filter = $"((cn={name}))";
                        /*ds.PropertiesToLoad.Add("cn");//first name
                        ds.PropertiesToLoad.Add("sn"); //last name*/
                        var result = ds.FindOne();
                        if (result != null)
                        {
                            foreach (DictionaryEntry item in result.Properties)
                            {
                                list.Add(new KeyValuePair<string, string>(item.Key.ToString()
                                    , string.Join(", ",
                                    ((ResultPropertyValueCollection)item.Value).Cast<string>())));

                            }
                        }
                    }
                }
            }
            catch (COMException ex)
            {
                Log.LogError(ex, $"COMException during AD search.");
            }
            return list;
        }
    }
}