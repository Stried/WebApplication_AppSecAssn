using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApplication3.ViewModels;

namespace WebApplication3.Model
{
    public class SecurityStampMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public SecurityStampMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user.SecurityStamp != context.User.FindFirstValue("AspNet.Identity.SecurityStamp")) 
                {
                    await signInManager.SignOutAsync();
                    context.Response.Redirect("/Login");
                    return;
                }
            }

            await _requestDelegate(context);
        }
    }
}
