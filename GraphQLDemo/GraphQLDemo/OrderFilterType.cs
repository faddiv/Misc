using HotChocolate.Data.Filters;
using NorthwindDatabase;

namespace GraphQLDemo;

public class OrderFilterType : FilterInputType<Orders>
{
    protected override void Configure(IFilterInputTypeDescriptor<Orders> descriptor)
    {
        descriptor.Field(t => t.OrderId)
            .Type<IdOperationFilterInputType>()
            .Name("id");
    }
}
