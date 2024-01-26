using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection.Metadata.Ecma335;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public LogoutModel(SignInManager<User> signInManager, UserManager<User> userManager, IHttpContextAccessor context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _contextAccessor = context;
        }

        public async void OnGet()
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

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("Login");
        }

        public async Task<IActionResult> OnPostDontLogoutAsync()
        {
            return RedirectToPage("Index");
        }
    }
}
