namespace ECommerceAPI.Models
{
    public class Cart
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } 
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
