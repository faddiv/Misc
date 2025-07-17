using GraphQLDemo;
using Microsoft.EntityFrameworkCore;
using NorthwindDatabase;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddPooledDbContextFactory<TestDbContext>(opt =>
    {
        opt
            .UseSqlServer(TestDbContextProvider.ConnectionString)
            .EnableSensitiveDataLogging();
    });
builder.Services
    .AddGraphQLServer()
    .RegisterDbContextFactory<TestDbContext>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddGlobalObjectIdentification()
    .AddGraphQLDemoTypes()
    .AddQueryType<Query>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL();

app.Run();
