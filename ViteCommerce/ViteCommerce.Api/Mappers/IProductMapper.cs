using Catalog.API.Entities;
using Mapster;
using System.Reflection;
using ViteCommerce.Api.Application.ProductGroup.PostProduct;

namespace ViteCommerce.Api.Mappers;

[Mapper]
public interface IProductMapper
{
    Product MapToProduct(PostProductCommand command);
}

public class ProductMapperConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PostProductCommand, Product>()
            .Map(e => e.Category, e => e.Category ?? "")
            .Map(e => e.Name, e => e.Name ?? "")
            .Ignore(e => e.Id);
    }
}

public class CodeGenerationRegister : ICodeGenerationRegister
{
    public void Register(CodeGenerationConfig config)
    {
        var as1 = this.GetType().Assembly;

        config.GenerateMapper("[name]Mapper")
            .ForAllTypesInNamespace(as1, "ViteCommerce.Api.Mappers");
    }
}
