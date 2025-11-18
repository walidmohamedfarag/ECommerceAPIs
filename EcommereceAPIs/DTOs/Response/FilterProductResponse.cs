namespace ECommerceAPI.DTOs.Response
{
    public record FilterProductResponse(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId, int? brandId, bool? lessQuantity);
}
