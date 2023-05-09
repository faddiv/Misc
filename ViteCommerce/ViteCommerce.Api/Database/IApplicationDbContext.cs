using Catalog.API.Entities;
using MongoDB.Driver;

namespace Database
{
    public interface IApplicationDbContext
    {
        IMongoCollection<Product> Products { get; }
    }
}