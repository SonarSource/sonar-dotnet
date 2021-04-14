namespace TestFrameworkExtensions
{
    using Microsoft.EntityFrameworkCore;

    public class NoncompliantDbContext : DbContext
    {
        private DbContextOptionsBuilder builder;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant {{Use a secure password when connecting to this database.}}
            optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            optionsBuilder.UseMySQL("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant

            builder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            builder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            builder.UseMySQL("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant

            optionsBuilder.UseSqlServer(sqlServerOptionsAction: null, connectionString: "Password="); // Noncompliant
        }

        protected void Method(DbContextOptionsBuilder<NoncompliantDbContext> genericBuilder)
        {
            genericBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            genericBuilder.UseNpgsql(npgsqlOptionsAction: null, connectionString: "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            genericBuilder.UseMySQL(MySQLOptionsAction: null, connectionString: "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
        }

        public void ExplicitInvocation(DbContextOptionsBuilder optionsBuilder)
        {
            SqlServerDbContextOptionsExtensions.UseSqlServer(optionsBuilder, "Password="); // Noncompliant
        }

        public void StringConcatAndInterpolation(string a)
        {
            builder.UseSqlServer("x" + "y" + "Password="); // Noncompliant
            builder.UseSqlServer("x" + "y" + "Password=" + a);
            builder.UseNpgsql($"Server = {a}; Database = {a}; User Id = {a}; Password ="); // Noncompliant
        }

        private const string USED_CONNECT_STRING = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="; // FN
        private const string NOT_USED = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=";
        public void ConstantPropagation()
        {
            var used = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=";
            var notUsed = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=";
            builder.UseMySQL(used);
            builder.UseNpgsql(USED_CONNECT_STRING);
        }

        public void MethodResult()
        {
            builder.UseMySQL(BuildConnection()); // FN
            static string BuildConnection() => "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=";
        }

        public void Compliant(DbContextOptionsBuilder optionsBuilder, string password)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/connection-string-syntax#windows-authentication
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=SSPI");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=True");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Trusted_Connection=true");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Trusted_Connection=yes");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=Foo");
        }
    }
}

namespace CustomCode
{
    using Microsoft.EntityFrameworkCore;

    public static class OptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseSqlServer(this DbContextOptionsBuilder optionsBuilder, string connectionString) => optionsBuilder;
        public static DbContextOptionsBuilder UseNpgsql(this DbContextOptionsBuilder optionsBuilder, string connectionString) => optionsBuilder;
        public static DbContextOptionsBuilder UseMySQL(this DbContextOptionsBuilder optionsBuilder, string connectionString) => optionsBuilder;
    }

    public class Foo
    {
        public void UseSqlServer(string connectionString) { }
    }
}

namespace TestCustomCode
{
    using CustomCode;

    public class NoncompliantDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        Foo foo;
        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseMySQL("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");

            foo.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
        }
    }
}
