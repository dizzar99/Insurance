using Insurance.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Insurance.DataAccess
{
    public class ApplicationContext : DbContext
    {
        public DbSet<DbSession> Sessions { get; set; }
        public DbSet<DbECDSAKey> ServerKeys { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            this.Database.EnsureCreated();
        }
    }
}
