namespace TestFrameworkExtensions
{
    using Microsoft.EntityFrameworkCore;

    public class NoncompliantDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer();
            optionsBuilder.UseNpgsql();
            optionsBuilder.UseSqlite();
            optionsBuilder.UseOracle();
            optionsBuilder.UseMySQL(); // Error [CS1501]
        }
    }
}
