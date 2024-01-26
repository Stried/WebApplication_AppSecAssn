using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication3.Pages
{
    [Authorize]
    public class AccountModel : PageModel
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AccountModel(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public void OnGet()
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
