using Microsoft.EntityFrameworkCore;
using CertServer.Models;
using CoreCA.DataModel;

namespace CertServer.Data
{
    public class IMoviesCAContext : DbContext
    {
        public IMoviesCAContext (DbContextOptions<IMoviesCAContext> options)
            : base(options)
        {
        }

        public DbSet<PublicCertificate> PublicCertificates { get; set; }
        public DbSet<PrivateKey> PrivateKeys { get; set; }

        // Fix capitalisation of table name
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PublicCertificate>()
                .ToTable(Constants.IMoviesCAPublicCertsTableName);
            modelBuilder.Entity<PrivateKey>()
                .ToTable(Constants.IMoviesCAPrivateCertsTableName);
        }
    }
}
