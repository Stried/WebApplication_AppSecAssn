using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.ViewModels;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly UserManager<User> _userManager;

        public HomeController(IHttpContextAccessor contextAccessor, UserManager<User> userManager)
        {
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public IActionResult Register()
        {
            return View();
        }
    }
}
