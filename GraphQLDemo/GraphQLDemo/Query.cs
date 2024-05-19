using NorthwindDatabase;

namespace GraphQLDemo;

public class Query
{
    public string Hello(string name)
    {
        return $"Hello {name}";
    }

    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Orders> GetOrders(TestDbContext context)
    {
        return context.Orders;
    }

    [UseOffsetPaging]
    public IQueryable<Orders> GetOrdersByOffset(TestDbContext context)
    {
        return context.Orders;
    }
}
