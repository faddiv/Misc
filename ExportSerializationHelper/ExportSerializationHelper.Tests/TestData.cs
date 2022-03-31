using Bogus;
using System;
using System.Collections.Generic;

namespace ExportSerializationHelper.Tests;

public class TestData
{
    public static List<ExampleClass> GenerateExampleClasses(int count = 10)
    {
        var today = DateTime.Parse("2022-03-06");
        var faker = new Faker<ExampleClass>()
        .UseSeed(1234)
        .RuleFor(e => e.Id, f => f.Random.Number(1000))
        .RuleFor(e => e.TextValue, f => f.Random.AlphaNumeric(10))
        .RuleFor(e => e.TimestampValue, f => f.Date.Recent(refDate: today))
        .RuleFor(e => e.NullableDate, f => f.Random.Bool() ? f.Date.Recent(refDate: today) : null);
        return faker.Generate(10);
    }
}
