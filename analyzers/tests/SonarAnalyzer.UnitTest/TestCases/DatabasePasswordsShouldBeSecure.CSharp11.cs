namespace TestFrameworkExtensions
{
    using System.Data.Common;
    using Microsoft.EntityFrameworkCore;

    public class NoncompliantDbContext : DbContext
    {
        private DbContextOptionsBuilder builder;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("""Password=Server=myServerAddress"""); // Compliant, inside we only look at 'Password=;'
            optionsBuilder.UseSqlServer("""Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="""); // Noncompliant {{Use a secure password when connecting to this database.}}
        }
    }
}
