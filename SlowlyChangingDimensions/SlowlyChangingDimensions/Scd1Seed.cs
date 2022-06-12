using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlowlyChangingDimensions
{
    public class Scd1Seed
    {
        public static void Seed()
        {
            var bog = new Faker<ScdExampleTable1>()
                .UseSeed(1000)
                .RuleFor(e => e.Data1, e => e.Name.FullName())
                .RuleFor(e => e.Data2, e => e.Name.JobTitle())
                .RuleFor(e => e.Data3, e => e.Company.CompanyName())
                .Ignore(e => e.Id)
                .Ignore(e => e.CorrelationId)
                .RuleFor(e => e.CreatedTimestamp, e => e.Date.Between(DateTime.Parse("2000.01.01"), DateTime.Parse("2000.12.31")))
                .Ignore(e => e.NextVersion)
                .Ignore(e => e.PreviousVersion)
                .Ignore(e => e.PreviousVersionId);
            var faker = ((IFakerTInternal)bog).FakerHub;
            var startList = bog.Generate(5000);
            for (int i = 0; i < startList.Count; i++)
            {
                startList[i].CorrelationId = i + 1;
            }
            using var db = new ScdDbContext1();
            db.ScdExampleTable1.AddRange(startList);
            db.SaveChanges();
            var startDate = startList.Min(e => e.CreatedTimestamp).AddDays(100);
            int counter = 0;
            for (int i = 0; i < 365*10; i++)
            {
                var now = startDate.AddDays(i);
                var elements = faker.PickRandom(startList.Where(e => e.CreatedTimestamp <= now), 50);
                foreach (var element in elements)
                {
                    var newElement = element.NewVersion(now);
                    newElement.Data2 = faker.Name.JobTitle();
                    newElement.Data3 = faker.Company.CompanyName();
                    db.ScdExampleTable1.Add(newElement);
                    counter++;
                    if (counter == 500)
                    {
                        db.SaveChanges();
                        counter = 0;
                    }
                    startList[startList.IndexOf(element)] = newElement;
                }
            }
            db.SaveChanges();
        }
    }
}
