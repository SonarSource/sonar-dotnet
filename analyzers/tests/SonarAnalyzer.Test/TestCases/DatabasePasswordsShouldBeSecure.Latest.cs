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

            string[] test = new string[] { "FirstTest", "SecondTest" };

            optionsBuilder.UseSqlServer($"""Server={test[0]};Database=myDataBase;User Id=myUsername;Password="""); // Noncompliant

            optionsBuilder.UseSqlServer($"Server={test
                [0]};Database=myDataBase;User Id=myUsername;Password="); // Noncompliant@-1 {{Use a secure password when connecting to this database.}}
            optionsBuilder.UseSqlServer($"Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password={test
                [1]}"); // Compliant

            optionsBuilder.UseSqlServer($$"""Server={{test
                [0]}};Database=myDataBase;User Id=myUsername;Password="""); // Noncompliant@-1
            optionsBuilder.UseSqlServer($$"""Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password={{test
                [1]}}"""); // Compliant

            var charLiteralEscape = '\e';
            optionsBuilder.UseSqlServer("Server=Server;Password=" + "\e"); // Compliant
            optionsBuilder.UseSqlServer("Server=Server;Password=" + "\easda"); // Compliant
            optionsBuilder.UseSqlServer("Server=Server;Password=" + charLiteralEscape); // Compliant
        }
    }
}
