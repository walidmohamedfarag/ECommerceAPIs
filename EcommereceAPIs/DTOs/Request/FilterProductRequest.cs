namespace ECommerceAPI.DTOs.Request
{
    public record FilterProductRequest(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool? lessQuantity);
}
