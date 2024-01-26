using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace WebApplication3.ViewModels
{
    public class Register
    {
        [Required, MinLength(8)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string CreditCardNo { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public int PhoneNumber { get; set; }

        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required, EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Upload)]
        public IFormFile PhotoString { get; set; } 

        [Required]
        public string AboutMe { get; set; } = string.Empty;

        [Required, MinLength(8, ErrorMessage = "Enter at least an eight character password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; } = string.Empty;


    }
}
