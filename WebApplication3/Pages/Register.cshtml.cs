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
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;
using System.Net;
using System.Text.Json;

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

                // Backend Check - Full Name
                string regexPatternFullName = "^[a-zA-Z ]+$";
                Regex regexFullName = new Regex(regexPatternFullName);
                if (RModel.FullName.Trim().Length == 0)
                {
                    ModelState.AddModelError("", "Full Name cannot be empty");
                }
                else if (RModel.FullName.Trim().Length < 8)
                {
                    ModelState.AddModelError("", "Full Name cannot be less than 8 Characters");
                }
                else if (!(regexFullName.Match(RModel.FullName).Success))
                {
                    ModelState.AddModelError("", "Full Name cannot contain special characters");
                }

                // Backend Check - Credit Card
                string regexPatternCreditCard = "^[0-9]+$";
                Regex regexCredCard = new Regex(regexPatternCreditCard);
                if (RModel.CreditCardNo.Length == 0)
                {
                    ModelState.AddModelError("", "Credit Card Number cannot be empty");
                }
                else if (RModel.CreditCardNo.Length != 16)
                {
                    ModelState.AddModelError("", "Credit Card Number must be 16 digits long.");
                }
                else if (!(regexCredCard.Match(RModel.CreditCardNo).Success))
                {
                    ModelState.AddModelError("", "Credit Card Number can only contain numbers.");
                }

                // Backend Check - Gender
                string regexPatternGender = "^[a-zA-Z]+$";
                Regex regexGender = new Regex(regexPatternGender);
                if (RModel.Gender.Length == 0)
                {
                    ModelState.AddModelError("", "Gender cannot be empty");
                }
                else if (RModel.Gender != "Male" || RModel.Gender != "Female")
                {
                    ModelState.AddModelError("", "Gender can only be 'Male' or 'Female'.");
                }
                else if (!(regexGender.Match(RModel.Gender).Success)) 
                {
                    ModelState.AddModelError("", "Gender cannot contain special characters.");
                }

                // Backend Check - Delivery Address
                string regexPatternDeliveryAddress = "^[a-zA-Z0-9 ]+$";
                Regex regexDeliveryAddress = new Regex(regexPatternDeliveryAddress);
                if (RModel.DeliveryAddress.Length == 0)
                {
                    ModelState.AddModelError("", "Delivery Address cannot be empty");
                }
                else if (!(regexDeliveryAddress.Match(RModel.DeliveryAddress).Success))
                {
                    ModelState.AddModelError("", "Delivery Address can only contain alphanumerical characters.");
                }

                if (ReCaptchaPassed() == false)
                {
                    ModelState.AddModelError("", "You failed the Captcha");
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

                if (result.Succeeded)
                {
                    // await signInManager.SignInAsync(user, false);
                    OldPasswordHash oldPasswordHist = new()
                    {
                        OldPassword = finalHash,
                        UserAccount = RModel.Email,
                    };

                    authDbContext.OldPasswordHashes.Add(oldPasswordHist);
                    authDbContext.SaveChanges();

                    return RedirectToPage("Login");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return Page();
        }


        public bool ReCaptchaPassed()
        {
            string Response = Request.Form["g-recaptcha-response"];
            bool Valid = false;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify?secret=6Lc17V0pAAAAACppEA7nOCdeqA6Ov4TqrqrODOxC&response={Response}");
            try
            {
                using (WebResponse wResponse = req.GetResponse())
                {
                    using (StreamReader readStream = new StreamReader(wResponse.GetResponseStream()))
                    {
                        string jsonResponse = readStream.ReadToEnd();

                        var data = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonResponse);

                        Valid = data.success;
                    }
                }

                return Valid;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }




    }
}
