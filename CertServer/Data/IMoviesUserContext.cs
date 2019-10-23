using Microsoft.EntityFrameworkCore;
using CertServer.Models;

namespace CertServer
{
    public class IMoviesUserContext : DbContext
    {
        public IMoviesUserContext(DbContextOptions<IMoviesUserContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}