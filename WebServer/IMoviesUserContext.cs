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
    }
}
