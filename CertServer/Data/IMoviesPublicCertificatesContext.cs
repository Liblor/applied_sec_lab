using Microsoft.EntityFrameworkCore;
using CertServer.Models;

namespace CertServer.Data
{
    public class IMoviesPublicCertificatesContext : DbContext
    {
        public IMoviesPublicCertificatesContext (DbContextOptions<IMoviesPublicCertificatesContext> options)
            : base(options)
        {
        }

        public DbSet<PublicCertificate> PublicCertificates { get; set; }
    }
}