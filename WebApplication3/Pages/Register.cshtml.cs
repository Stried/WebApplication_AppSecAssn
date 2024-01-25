using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.ViewModels;
using NanoidDotNet;
using WebApplication3.Model;

namespace WebApplication3.Pages
{
    public class RegisterModel : PageModel
    {

        private UserManager<User> userManager { get; }
        private SignInManager<User> signInManager { get; }
        private readonly AuthDbContext authDbContext;
        private readonly ILogger<RegisterModel> logger;


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
                // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-7.0
                byte[] photoBytes = null;
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
                    CreditCardNo = RModel.CreditCardNo,
                    Gender = RModel.Gender,
                    PhoneNumber = RModel.PhoneNumber,
                    DeliveryAddress = RModel.DeliveryAddress,
                    Email = RModel.Email,
                    PhotoString = photoBytes,
                    AboutMe = RModel.AboutMe,
                    Password = RModel.Password,

                };
                var result = await userManager.CreateAsync(user, RModel.Password);
                authDbContext.SaveChangesAsync();
                logger.LogError("Result", result);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);

                    return RedirectToPage("Index");
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
