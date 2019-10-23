using Microsoft.EntityFrameworkCore;
using CertServer.Models;

namespace CertServer.Data
{
    public class IMoviesPrivateKeysContext : DbContext
    {
        public IMoviesPrivateKeysContext (DbContextOptions<IMoviesPrivateKeysContext> options)
            : base(options)
        {
        }

        public DbSet<PrivateKey> PrivateKeys { get; set; }
    }
}