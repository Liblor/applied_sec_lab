using CertServer.Models;
using CoreCA.DataModel;
using Microsoft.EntityFrameworkCore;

namespace CertServer
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
				.ToTable(CAConfig.IMoviesUserTableName);
		}
    }
}