using Auth.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.DataAccess
{
    public class ApplicationContext : DbContext
    {
        //public DbSet<User> Users { get; set; }
        //public DbSet<UserCredentials> UserCredentials { get; set; }
        public DbSet<DbSession> Sessions { get; set; }
        public DbSet<DbECDSAKey> ServerKeys { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            this.Database.EnsureCreated();
        }
    }
}
