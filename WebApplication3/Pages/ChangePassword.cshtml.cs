using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private UserManager<User> _userManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly IConfiguration _configuration;
        private AuthDbContext _authDbContext;
        private static string salt;
        private static string finalHash;

        [BindProperty]
        public ChangePassword CModel { get; set; }

        public ChangePasswordModel(UserManager<User> userManager, ILogger<ChangePasswordModel> logger, AuthDbContext authDbContext, IConfiguration configuration)
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
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogInformation("User is infact, Null");
                    ModelState.AddModelError("", "User is null");
                }

                // Hash for password, UserManager hash seems to return a different hash each time
                byte[] saltByte = new byte[8];
                salt = getDBSalt(user.Email);

                SHA512Managed hashing = new SHA512Managed();

                string pwdWithSalt = CModel.NewPassword + salt;
                byte[] plainHash = hashing.ComputeHash(Encoding.UTF8.GetBytes(CModel.NewPassword));
                byte[] hashWithSalt = hashing.ComputeHash(Encoding.UTF8.GetBytes(pwdWithSalt));

                finalHash = Convert.ToBase64String(hashWithSalt);
                // Hashing Stops

                var newUserPasswordHash = _userManager.PasswordHasher.HashPassword(user ,CModel.NewPassword);
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
                        UserEmail = user.Email,
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
            } else
            {
                return RedirectToPage("Account");
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
