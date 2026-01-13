using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Common_Authentication.Models;


namespace Common_Authentication
{
    public class AuthenticationDBContext : IdentityDbContext<ApplicationUser>    {
        public AuthenticationDBContext(DbContextOptions<AuthenticationDBContext> options)
            : base(options)
        {
        }
    }
}
