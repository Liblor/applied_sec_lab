using CoreCA.DataModel;
using Microsoft.EntityFrameworkCore;
using WebServer.Models;

namespace WebServer
{
    public class IMoviesUserContext : DbContext
    {
        public IMoviesUserContext(DbContextOptions<IMoviesUserContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }

        // Fix capitalisation of table name
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable(Constants.IMoviesUserTableName);
            modelBuilder.Entity<Admin>()
                .ToTable(Constants.IMoviesAdminTableName);
        }
    }
}
