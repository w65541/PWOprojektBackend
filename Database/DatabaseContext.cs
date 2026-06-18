using Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class DatabaseContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<UserModificationLog> UserModificationLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }


    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tell EF Core this table has a trigger
           /* modelBuilder.Entity<User>()
                .ToTable("Users", tb => tb.HasTrigger("userModificationLog"));*/
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
      var connectionString = "Server=localhost;database=UserManagement;User=root;Password=root;";
      optionsBuilder
          .UseLazyLoadingProxies()
          .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
     /* optionsBuilder
                .UseLazyLoadingProxies()
                .UseSqlServer(@"Server=localhost\SQLEXPRESS01;TrustServerCertificate=True;Database=Database;Trusted_Connection=True;user=admin'password=admin");*/
        }
    }
}
