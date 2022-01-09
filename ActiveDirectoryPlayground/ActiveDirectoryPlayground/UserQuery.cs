using System.DirectoryServices;

namespace ActiveDirectoryPlayground
{
    internal class UserQuery
    {
        public static void Execute()
        {

            var r = new DirectoryEntry("LDAP://FADDIV:50000/CN=Users,CN=faddiv,DC=localtest,DC=me");

            //var s = new DirectorySearcher(r, "(displayName=Fad*)", new[] { "displayName", "employeeId", "c", "mail" }, SearchScope.Subtree);
            foreach (DirectoryEntry item in r.Children)
            {
                Console.WriteLine("{0} - {1} - {2} - {3}",
                    item.Properties["displayName"][0],
                    item.Properties["employeeId"][0],
                    item.Properties["c"][0],
                    item.Properties["mail"][0]);
            }

        }
    }
}
