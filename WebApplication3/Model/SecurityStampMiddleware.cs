using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApplication3.ViewModels;

namespace WebApplication3.Model
{
    public class SecurityStampMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public SecurityStampMiddleware(RequestDelegate requestDelegate, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _requestDelegate = requestDelegate;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task Invoke(HttpContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);

                var sessionItem = _contextAccessor.HttpContext.Session;
                var sessSecurityStamp = sessionItem.GetString("SecurityStamp");
                if (sessSecurityStamp != null || sessSecurityStamp != user.SecurityStamp)
                {
                    await _signInManager.SignOutAsync();
                    sessionItem.Clear();
                    context.Response.Redirect("/Login");
                }
            }

            await _requestDelegate(context);
        }
    }
}
