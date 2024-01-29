using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class ForgetPasswordModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }
        
        private IHttpContextAccessor _contextAccessor;
        private UserManager<User> _userManager;

        public ForgetPasswordModel(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _contextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;

            smtpClient.Credentials = new NetworkCredential("ecolife.userTest@gmail.com", "pkgasnfsumpudfik");

            MailAddress mailAddress = new MailAddress("ecolife.userTest@gmail.com");
            MailAddress to = new MailAddress(Email);
            MailMessage message = new MailMessage(mailAddress, to);

            var userDetails = await _userManager.FindByNameAsync(Email);
            var secStamp = Guid.NewGuid().ToString();
            userDetails.SecurityStamp = secStamp;
            await _userManager.UpdateAsync(userDetails);

            message.Body = $"Click on this link if you have forgotten your password https://localhost:44358/ResetPassword?email={Email}&v={secStamp}";
            message.Subject = "Verification Code";

            string userState = "testMessage1";
            smtpClient.Send(message);

            return RedirectToPage("Index");
        }
    }
}
