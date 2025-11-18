namespace ECommerceAPI.DTOs.Response
{
    public class ProductResponse
    {
        public Product product { get; set; } = null!;
        public IEnumerable<ProductSubImage>? ProductSubImages { get; set; }
        public List<Product>? RealatedProduct { get; set; }

        public IEnumerable<ProductColor>? productColors { get; set; }
    }
}
