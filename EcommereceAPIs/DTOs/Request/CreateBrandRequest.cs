namespace ECommerceAPI.DTOs.Request
{
    public class CreateBrandRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public IFormFile? Image { get; set; }

    }
}
