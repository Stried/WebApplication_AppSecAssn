using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels
{
    public class User : IdentityUser
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string CreditCardNo { get; set; }

        // Ensures only male or female is entered
        [EnumDataType(typeof(GenderEnum))]
        public string Gender { get; set; } = string.Empty;

        public string PhoneNumber {  get; set; }

        public string DeliveryAddress { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public byte[] PhotoString { get; set; }

        public string AboutMe { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string SecurityStamp {  get; set; } = string.Empty;

        public string SaltyBites {  get; set; } = string.Empty;
    }

    public enum GenderEnum
    {
        Male,
        Female
    }
}
