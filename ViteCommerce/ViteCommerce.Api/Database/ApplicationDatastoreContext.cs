
using Catalog.API.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;

namespace Database;

public class ApplicationDbContext : IApplicationDbContext
{
    private IMongoDatabase _database;

    public ApplicationDbContext(IMongoClient client)
    {
        _database = client.GetDatabase("ViteCommerceDB");

        Products = _database.GetCollection<Product>("products");
    }

    public IMongoCollection<Product> Products { get; }
}
