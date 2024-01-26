using Microsoft.AspNetCore.Identity;

namespace WebApplication3.ViewModels
{
    public class User : IdentityUser
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string CreditCardNo { get; set; }

        public string Gender { get; set; } = string.Empty;

        public int PhoneNumber {  get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public byte[] PhotoString { get; set; }

        public string AboutMe { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string SaltyBites {  get; set; } = string.Empty;
    }
}
