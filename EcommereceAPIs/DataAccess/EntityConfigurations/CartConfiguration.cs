using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceAPI.DataAccess.EntityConfigurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {

        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.HasKey(k => new { k.ProductId, k.UserId });
        }
    }
}
