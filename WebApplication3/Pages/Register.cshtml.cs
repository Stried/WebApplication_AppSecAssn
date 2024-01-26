using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.ViewModels;
using NanoidDotNet;
using WebApplication3.Model;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using System.Web;

namespace WebApplication3.Pages
{
    public class RegisterModel : PageModel
    {

        private UserManager<User> userManager { get; }
        private SignInManager<User> signInManager { get; }
        private readonly AuthDbContext authDbContext;
        private readonly ILogger<RegisterModel> logger;
        private static string salt;
        private static string finalHash;


        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(UserManager<User> userManager,
        SignInManager<User> signInManager,
        AuthDbContext dbContext,
        ILogger<RegisterModel> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authDbContext = dbContext;
            this.logger = logger;
        }



        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
                var protector = dataProtectionProvider.CreateProtector("MySecretKey");

                // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-7.0
                byte[] photoBytes = null;
                byte[] saltByte = new byte[8];

                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(saltByte);
                salt = Convert.ToBase64String(saltByte);

                SHA512Managed hashing = new SHA512Managed();

                string pwdWithSalt = RModel.Password + salt;
                byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(RModel.Password));
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                finalHash = Convert.ToBase64String(hashWithSalt);


                using (MemoryStream photoStream = new MemoryStream())
                {
                    await RModel.PhotoString.CopyToAsync(photoStream);
                    photoBytes = photoStream.ToArray();
                    // Check if i need to use Nanoid to store images
                }

                var user = new User()
                {
                    UserName = RModel.Email,
                    FullName = RModel.FullName,
                    CreditCardNo = protector.Protect(RModel.CreditCardNo),
                    Gender = RModel.Gender,
                    PhoneNumber = RModel.PhoneNumber,
                    DeliveryAddress = RModel.DeliveryAddress,
                    Email = RModel.Email,
                    PhotoString = photoBytes,
                    AboutMe = HttpUtility.HtmlEncode(RModel.AboutMe),
                    Password = finalHash,
                    SaltyBites = salt,

                };
                var result = await userManager.CreateAsync(user, RModel.Password);
                authDbContext.SaveChangesAsync();
                logger.LogError("Result", result);

                if (result.Succeeded)
                {
                    // await signInManager.SignInAsync(user, false);

                    return RedirectToPage("Login");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return Page();
        }







    }
}
