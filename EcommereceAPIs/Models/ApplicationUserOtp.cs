namespace ECommerceAPI.Models
{
    public class ApplicationUserOtp
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string OtpCode { get; set; }
        public DateTime ExpirationTime { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsValid { get; set; }
    }
}
