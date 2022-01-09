using Bogus;
using System.DirectoryServices;

namespace ActiveDirectoryPlayground
{
    internal class UserGenerator
    {
        public static void Generate()
        {
            var r = new DirectoryEntry("LDAP://FADDIV:50000/CN=Users,CN=faddiv,DC=localtest,DC=me");

            var faker = new Faker();
            for (int i = 0; i < 100; i++)
            {
                var person = new Person();
                var de = r.Children.Add("CN=" + person.UserName, "user");
                de.Properties["displayName"].Add(person.FullName);
                de.Properties["employeeId"].Add(string.Join("", faker.Random.Digits(8)));
                de.Properties["c"].Add(faker.Address.CountryCode());
                de.Properties["mail"].Add(person.Email);
                de.CommitChanges();
            }
        }
    }
}
