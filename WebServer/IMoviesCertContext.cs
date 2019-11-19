using Microsoft.EntityFrameworkCore;
using CoreCA.DataModel;

namespace WebServer
{
    public class IMoviesCertContext : DbContext
    {
        public IMoviesCertContext(DbContextOptions<IMoviesCertContext> options)
            : base(options)
        {
        }

        public DbSet<PublicCertificate> PublicCertificates { get; set; }

        // Fix capitalisation of table name
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PublicCertificate>()
                .ToTable(Constants.IMoviesCAPublicCertsTableName);
        }
    }
}