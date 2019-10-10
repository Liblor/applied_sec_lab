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
                    Id = "test.user@imovies.local",
                    Email = "test.user@imovies.local",
                    FirstName = "Test",
                    LastName = "User"
                });

                context.SaveChanges();
            }
        }
    }
}
