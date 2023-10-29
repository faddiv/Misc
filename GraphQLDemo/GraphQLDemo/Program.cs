using Bogus;
using HotChocolate.Types.Pagination;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL();

app.Run();


public class Query
{
    public string Hello(string name)
    {
        return $"Hello {name}";
    }

    [UseOffsetPaging]
    public CollectionSegment<User> Users(int generate = 100, int skip = 0, int take = 10)
    {
        var faker = new Faker<User>()
            .UseSeed(42)
            .RuleFor(e => e.FullName, e => e.Name.FullName());
        //return faker.Generate(generate);
        return new CollectionSegment<User>(
            faker.Generate(generate).Skip(skip).Take(take).ToList(),
            new CollectionSegmentInfo(true, true), generate);
    }

    [UsePaging]
    public Connection<User> Users2(int generate = 100, string? after = null, string? before = null)
    {
        var faker = new Faker<User>()
            .UseSeed(42)
            .RuleFor(e => e.FullName, e => e.Name.FullName());
        return new Connection<User>(
            faker
                .Generate(generate)
                .Skip(10)
                .Take(10)
                .Select((e, i) =>
                {
                    return new Edge<User>(e, Convert.ToBase64String(Encoding.UTF8.GetBytes($"{i}")));
                }).ToList()
            , new ConnectionPageInfo(true, true, "Bla", "BlaBla"), generate);
    }
}

public class User
{
    public string FullName { get; set; }
}
