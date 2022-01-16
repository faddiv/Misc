using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ActiveDirectoryPlayground
{
    public class PointlesLinqTests
    {
        [Fact]
        public void CanQueryAList()
        {
            var data = RandomData(100);
            var query = new PointlessLinqQueryable<LdapUser>(data);
            var result = query.ToList();
            result.Should().HaveCount(100);
        }
        [Fact]
        public void CanQueryFirst()
        {
            var data = RandomData(100);
            var query = new PointlessLinqQueryable<LdapUser>(data);
            var result = query.FirstOrDefault();
            result.Should().Be(data[0]);
        }

        private static List<LdapUser> RandomData(int count)
        {
            var groups = new string[] { "User", "Administrator", "Reader" };
            return new Bogus.Faker<LdapUser>()
                .RuleFor(e => e.CommonName, f => f.Person.UserName)
                .RuleFor(e => e.Country, f => f.Company.Locale)
                .RuleFor(e => e.DisplayName, f => f.Person.FullName)
                .RuleFor(e => e.DistungishedName, f => f.Person.UserName)
                .RuleFor(e => e.EmployeeId, f => f.Random.String(5))
                .RuleFor(e => e.Mail, f => f.Person.Email)
                .RuleFor(e => e.ObjectGuid, f => f.Random.Guid())
                .RuleFor(e => e.MemberOf, faker => faker.Random.ListItems(groups, faker.Random.Int(0, 3)).ToArray())
                .Generate(count);
        }
    }
}
