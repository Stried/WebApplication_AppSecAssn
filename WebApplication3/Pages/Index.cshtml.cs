using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public IndexModel(SignInManager<User> signInManager, UserManager<User> userManager, IHttpContextAccessor context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _contextAccessor = context;
        }

        public async void OnGet()
        {
            
        }

        public async Task Load()
        {
            if (_contextAccessor.HttpContext.Session.IsAvailable)
            {
                var sessionItem = _contextAccessor.HttpContext.Session;

                var user = await _userManager.FindByEmailAsync(sessionItem.GetString("Email"));
                var sessSecurityStamp = sessionItem.GetString("SecurityStamp");
                if (sessSecurityStamp != null || sessSecurityStamp != user.SecurityStamp)
                {
                    await _signInManager.SignOutAsync();
                    sessionItem.Clear();
                    RedirectToPage("/Login");
                }
            }
        }
    }
}