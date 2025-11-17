using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.EntityConfigurations
{
    public class ProductSubImageConfiguration : IEntityTypeConfiguration<ProductSubImage>
    {
        public void Configure(EntityTypeBuilder<ProductSubImage> builder)
        {
            builder.HasKey(k => new { k.ProductId, k.Imge });
        }
    }
}
