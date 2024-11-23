using Microsoft.EntityFrameworkCore;
using MyServer.Models;
using OpenIddict.EntityFrameworkCore.Models;

namespace MyServer.Data
{
    

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }

        // Стандартная таблица для OpenIddict
        public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications { get; set; }
        public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations { get; set; }
        public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes { get; set; }
        public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; }
    }

}
