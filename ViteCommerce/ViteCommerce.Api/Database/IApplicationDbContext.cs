using MongoDB.Driver;
using ViteCommerce.Api.Entities;

namespace Database
{
    public interface IApplicationDbContext
    {
        IMongoCollection<Product> Products { get; }
    }
}
