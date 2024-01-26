using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApplication3.ViewModels;

namespace WebApplication3.Model
{



    public class AuthDbContext : IdentityDbContext<ViewModels.User>
    {
        private readonly IConfiguration _configuration;
        public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration) : base(options) 
        {
            _configuration = configuration;
            string connectionString = _configuration.GetConnectionString("AuthConnectionString");
            
        }
        //public AuthDbContext(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _configuration.GetConnectionString("AuthConnectionString");
            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<AuditLog> AuditLog { get; set; }
    }




}
