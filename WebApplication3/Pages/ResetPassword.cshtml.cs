using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class ResetPasswordModel : PageModel
    {
        public string Email { get; set; }
        public string SecStamp { get; set; }

        [BindProperty]
        public string Password { get; set; }

        private UserManager<User> _userManager;
        private ILogger<ResetPasswordModel> _logger;
        private AuthDbContext _authDbContext;
        private IConfiguration _configuration;
        private static string salt;
        private static string finalHash;

        public ResetPasswordModel(UserManager<User> userManager, ILogger<ResetPasswordModel> logger, AuthDbContext authDbContext, IConfiguration configuration) 
        { 
            _userManager = userManager;
            _logger = logger;
            _authDbContext = authDbContext;
            _configuration = configuration;
        }

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Email = Request.Query["email"];
            SecStamp = Request.Query["v"];

            var user = await _userManager.FindByNameAsync(Email);

            if (user.SecurityStamp != SecStamp)
            {
                ModelState.AddModelError("", "Invalid link, please try again!");

                return Page();
            }

            byte[] saltByte = new byte[8];
            salt = getDBSalt(user.Email);

            SHA512Managed hashing = new SHA512Managed();

            string pwdWithSalt = Password + salt;
            byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(Password));
            byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

            finalHash = Convert.ToBase64String(hashWithSalt);
            // Hashing Stops

            var newUserPasswordHash = _userManager.PasswordHasher.HashPassword(user, Password);
            user.PasswordHash = newUserPasswordHash;

            var checkOldPassword = _authDbContext.OldPasswordHashes.Where(u => u.UserAccount == user.Email).Select(u => u.OldPassword).ToList();
            for (int i = 0; i < checkOldPassword.Count; i++)
            {
                _logger.LogInformation(checkOldPassword[i]);
                if (checkOldPassword[i] == finalHash)
                {
                    ModelState.AddModelError("", "Password cannot be same as last 2 passwords!");
                    return Page();
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                AuditLog newLog = new()
                {
                    UserEmail = Email,
                    Action = "Reset Password"
                };

                _authDbContext.AuditLog.Add(newLog);
                _authDbContext.SaveChanges();

                // await signInManager.SignInAsync(user, false);
                _logger.LogInformation($"{result.Succeeded}");
                OldPasswordHash oldPasswordHist = new()
                {
                    OldPassword = finalHash,
                    UserAccount = user.Email
                };

                _authDbContext.OldPasswordHashes.Add(oldPasswordHist);
                _authDbContext.SaveChanges();

                checkOldPassword = _authDbContext.OldPasswordHashes.Where(u => u.UserAccount == user.Email).Select(u => u.OldPassword).ToList();
                _logger.LogInformation(checkOldPassword.ToString());
                if (checkOldPassword.Count() > 2)
                {
                    _authDbContext.OldPasswordHashes.Remove(_authDbContext.OldPasswordHashes.Where(u => u.UserAccount == user.Email).First());
                    _authDbContext.SaveChanges();
                }

                return RedirectToPage("Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return Page();
        }

        protected string getDBSalt(string userEmail)
        {
            string s = null;

            SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("AuthConnectionString"));
            string sqlStatement = "select SaltyBites FROM AspNetUsers WHERE Email=@userEmail";
            SqlCommand command = new SqlCommand(sqlStatement, connection);
            command.Parameters.AddWithValue("@userEmail", userEmail);

            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["SaltyBites"] != null)
                        {
                            if (reader["SaltyBites"] != DBNull.Value)
                            {
                                s = reader["SaltyBites"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally { connection.Close(); }
            return s;
        }
    }
}
