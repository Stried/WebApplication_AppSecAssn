using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class LoginWithMFAModel : PageModel
    {
        [BindProperty(Name = "email")]
        public string Email { get; set; }

        [BindProperty(Name = "code")]
        public string TFAVerificationCode { get; set; }

        [BindProperty]
        public string InputCode { get; set; }

        private AuthDbContext _authDbContext;
        private UserManager<User> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public LoginWithMFAModel(AuthDbContext authDbContext, UserManager<User> userManager, IHttpContextAccessor contextAccessor)
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (InputCode != null)
            {
                if (InputCode == TFAVerificationCode)
                {
                    AuditLog newLog = new()
                    {
                        UserEmail = Email,
                        Action = "Login w. 2FA"
                    };

                    _authDbContext.AuditLog.Add(newLog);
                    _authDbContext.SaveChanges();

                    var userDetails = await _userManager.FindByNameAsync(Email);
                    userDetails.SecurityStamp = Guid.NewGuid().ToString();
                    await _userManager.UpdateAsync(userDetails);

                    _contextAccessor.HttpContext.Session.SetString("FullName", userDetails.FullName);
                    _contextAccessor.HttpContext.Session.SetString("CreditCardNo", userDetails.CreditCardNo.ToString());
                    _contextAccessor.HttpContext.Session.SetString("Gender", userDetails.Gender);
                    _contextAccessor.HttpContext.Session.SetString("PhoneNumber", userDetails.PhoneNumber);
                    _contextAccessor.HttpContext.Session.SetString("DeliveryAddress", userDetails.DeliveryAddress);
                    _contextAccessor.HttpContext.Session.SetString("Email", userDetails.Email);
                    _contextAccessor.HttpContext.Session.SetString("PhotoString", userDetails.PhotoString.ToString());
                    _contextAccessor.HttpContext.Session.SetString("AboutMe", userDetails.AboutMe);
                    _contextAccessor.HttpContext.Session.SetString("SecurityStamp", userDetails.SecurityStamp);

                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Verification Code provided is invalid");
                    return Page();
                }
            }

            return Page();
        }
    }
}
