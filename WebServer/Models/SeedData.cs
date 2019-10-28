using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace WebServer.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new IMoviesUserContext(
                serviceProvider.GetRequiredService<DbContextOptions<IMoviesUserContext>>()))
            {
                if (context.Users.Any())
                    return;

                context.Users.Add(new User
                {
                    Id = "test",
                    Email = "test.user@imovies.local",
                    FirstName = "Test",
                    LastName = "User",
                    // PasswordHash = SHA1("test")
                    PasswordHash = "a94a8fe5ccb19ba61c4c0873d391e987982fbbd3"
                });

                context.SaveChanges();
            }
        }
    }
}
