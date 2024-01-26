using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    [Authorize]
    public class AccountModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public AccountModel(SignInManager<User> signInManager, UserManager<User> userManager, IHttpContextAccessor context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _contextAccessor = context;
        }

        public async void OnGet()
        {
            
        }

        public void GetDetails()
        {
            var dataProtectionProvider = DataProtectionProvider.Create("EncryptData");
            var protector = dataProtectionProvider.CreateProtector("MySecretKey");

            var sessionItem = _contextAccessor.HttpContext.Session;

            var creditCardNo = _contextAccessor.HttpContext.Session.GetString("CreditCardNo");
            var credCardDetails = protector.Unprotect(creditCardNo);
        }
    }
}
