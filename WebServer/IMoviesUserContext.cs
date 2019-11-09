using CoreCA.DataModel;
using Microsoft.EntityFrameworkCore;

namespace WebServer
{
    public class IMoviesUserContext : DbContext
    {
        public IMoviesUserContext(DbContextOptions<IMoviesUserContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        // Fix capitalisation of table name
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable(Constants.IMoviesUserTableName);
        }
    }
}
