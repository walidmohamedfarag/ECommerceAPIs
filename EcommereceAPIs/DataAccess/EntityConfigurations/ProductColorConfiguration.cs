using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.EntityConfigurations
{
    public class ProductColorConfiguration : IEntityTypeConfiguration<ProductColor>
    {
        public void Configure(EntityTypeBuilder<ProductColor> builder)
        {
            builder.HasKey(k => new { k.Color, k.ProductId });
        }
    }
}
