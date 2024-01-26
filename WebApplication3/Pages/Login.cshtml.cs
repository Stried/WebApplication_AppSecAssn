using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;
using WebApplication3.Model;
using WebApplication3.ViewModels;

namespace WebApplication3.Pages
{
    [ValidateAntiForgeryToken]
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
        private readonly IConfiguration _config;
		private readonly IHttpContextAccessor _contextAccessor;
        private readonly AuthDbContext _authDbContext;
        private byte[] Key;
        private byte[] IV;

        public LoginModel(SignInManager<User> signInManager, IConfiguration configuration, IHttpContextAccessor contextAccessor, UserManager<User> userManager, AuthDbContext authDbContext)
        {
            _signInManager = signInManager;
            _config = configuration;
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _authDbContext = authDbContext;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {

				//var identityResult = await _signInManager.PasswordSignInAsync(LModel.Email, userHash, LModel.RememberMe, false);
				var identityResult = await _signInManager.PasswordSignInAsync(LModel.Email, LModel.Password, LModel.RememberMe, true);
				if (identityResult.Succeeded)
				{
                    AuditLog newLog = new()
                    {
                        UserEmail = LModel.Email,
                        Action = "Login"
                    };

                    _authDbContext.AuditLog.Add(newLog);
                    _authDbContext.SaveChanges();

                    var userDetails = await _userManager.FindByNameAsync(LModel.Email);
                    userDetails.SecurityStamp = Guid.NewGuid().ToString();
                    await _userManager.UpdateAsync(userDetails);

                    _contextAccessor.HttpContext.Session.SetString("FullName", userDetails.FullName);
                    _contextAccessor.HttpContext.Session.SetString("CreditCardNo", userDetails.CreditCardNo.ToString());
					_contextAccessor.HttpContext.Session.SetString("Gender", userDetails.Gender);
                    _contextAccessor.HttpContext.Session.SetInt32("PhoneNumber", userDetails.PhoneNumber);
                    _contextAccessor.HttpContext.Session.SetString("DeliveryAddress", userDetails.DeliveryAddress);
                    _contextAccessor.HttpContext.Session.SetString("Email", userDetails.Email);
                    _contextAccessor.HttpContext.Session.SetString("PhotoString", userDetails.PhotoString.ToString());
                    _contextAccessor.HttpContext.Session.SetString("AboutMe", userDetails.AboutMe);
                    _contextAccessor.HttpContext.Session.SetString("SecurityStamp", userDetails.SecurityStamp);

                    

                    return RedirectToPage("Index");
				}
					
				if (identityResult.IsLockedOut)
				{
                    AuditLog newLog = new()
                    {
                        UserEmail = LModel.Email,
                        Action = "LoginFail",
                        CreatedDate = DateTime.Now,
                    };

                    _authDbContext.AuditLog.Add(newLog);
                    _authDbContext.SaveChanges();
                    ModelState.AddModelError("", "Account is Locked Out. Please Try Again Later!");
				}
					
			
				ModelState.AddModelError("", "Username or Password Incorrect");
			}



            return Page();
        }

        protected string getDBHash(string userEmail)
        {
            string h = null;

            SqlConnection connection = new SqlConnection(_config.GetConnectionString("AuthConnectionString"));
            string sqlStatement = "select Password FROM AspNetUsers WHERE Email=@userEmail";
            SqlCommand command = new SqlCommand(sqlStatement, connection);
            command.Parameters.AddWithValue("@userEmail", userEmail);

            try
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Password"] != null)
                        {
                            if (reader["Password"] != DBNull.Value)
                            {
                                h = reader["Password"].ToString();
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
            return h;
        }

        protected string getDBSalt(string userEmail)
        {
			string s = null;

			SqlConnection connection = new SqlConnection(_config.GetConnectionString("AuthConnectionString"));
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

        protected byte[] encryptData(string data)
        {
			byte[] cipherText = null;
			try
			{
				RijndaelManaged cipher = new RijndaelManaged();
				cipher.IV = IV;
				cipher.Key = Key;
				ICryptoTransform encryptTransform = cipher.CreateEncryptor();
				//ICryptoTransform decryptTransform = cipher.CreateDecryptor();
				byte[] plainText = Encoding.UTF8.GetBytes(data);
				cipherText = encryptTransform.TransformFinalBlock(plainText, 0,
				plainText.Length);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.ToString());
			}
			finally { }
            return cipherText;

		}
    }
}
