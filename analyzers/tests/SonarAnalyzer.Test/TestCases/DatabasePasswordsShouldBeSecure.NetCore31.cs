namespace TestFrameworkExtensions
{
    using Microsoft.EntityFrameworkCore;

    public class NoncompliantDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(); // Error [CS1501]
            optionsBuilder.UseNpgsql(); // Error [CS1501]
            optionsBuilder.UseSqlite(); // Error [CS1501]
            optionsBuilder.UseOracle(); // Error [CS1501]
            optionsBuilder.UseMySQL(); // Error [CS1501]
        }
    }
}
