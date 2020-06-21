using System.DirectoryServices;

namespace WinAuth.Controllers
{
    public class LdapConfig
    {
        public string Path { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public AuthenticationTypes AuthenticationType { get; set; }
    }
}