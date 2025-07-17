using NorthwindDatabase;

namespace GraphQLDemo;

public class OrderType : ObjectType<Orders>
{
    protected override void Configure(IObjectTypeDescriptor<Orders> descriptor)
    {
        descriptor
            .ImplementsNode()
            .IdField(t => t.OrderId)
            .ResolveNode(async (ctx, id) =>
            {
                var db = ctx.Service<TestDbContext>();
                return await db.Orders.FindAsync(id);
            });
    }
}
