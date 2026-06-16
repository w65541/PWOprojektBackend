using Database;
using Database.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ApiTests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace real DB with in-memory
                services.RemoveAll<DbContextOptions<DatabaseContext>>();
                services.RemoveAll<DatabaseContext>();

                services.AddDbContext<DatabaseContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                    options.UseLazyLoadingProxies();
                });

                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                db.Database.EnsureCreated();
                SeedDatabase(db);
            });

            builder.UseEnvironment("Development");
        }

        private static void SeedDatabase(DatabaseContext db)
        {
            if (db.UserTypes.Any()) return;

            var adminType = new UserType { Id = 1, Name = "Admin" };
            var userType  = new UserType { Id = 2, Name = "User" };
            db.UserTypes.AddRange(adminType, userType);

            var admin = new User
            {
                Id = 1,
                Login = "admin",
                Haslo = "admin123",
                Email = "admin@test.com",
                TypeId = 1,
                IsActive = true,
                CreationDate = new DateTime(2024, 1, 1)
            };
            var user1 = new User
            {
                Id = 2,
                Login = "testuser",
                Haslo = "pass123",
                Email = "testuser@test.com",
                TypeId = 2,
                IsActive = true,
                CreationDate = new DateTime(2024, 6, 1)
            };
            var archivedUser = new User
            {
                Id = 3,
                Login = "archived",
                Haslo = "arch123",
                Email = "archived@test.com",
                TypeId = 2,
                IsActive = false,
                CreationDate = new DateTime(2024, 1, 10),
                ArchiveDate = new DateTime(2025, 1, 15),
                ArchiverId = 1
            };

            db.Users.AddRange(admin, user1, archivedUser);
            db.SaveChanges();
        }
    }
}
