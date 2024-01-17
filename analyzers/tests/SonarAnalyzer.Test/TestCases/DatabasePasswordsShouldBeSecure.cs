namespace TestFrameworkExtensions
{
    using System.Data.Common;
    using Microsoft.EntityFrameworkCore;

    public class NoncompliantDbContext : DbContext
    {
        private DbContextOptionsBuilder builder;
        private DbContextOptionsBuilder getBuilder() => null;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant {{Use a secure password when connecting to this database.}}
            optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            optionsBuilder.UseMySQL("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            optionsBuilder.UseSqlite("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            optionsBuilder.UseOracle("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant

            builder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            builder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            builder.UseMySQL("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password="); // Noncompliant
            builder.UseOracle("aPassword="); // Noncompliant FP
            builder.UseMySQL("aPassword=;"); // Noncompliant FP

            optionsBuilder.UseSqlServer(sqlServerOptionsAction: null, connectionString: "Password="); // Noncompliant

            getBuilder().UseSqlServer("Password="); // Noncompliant

            optionsBuilder.UseSqlite("Password=Server=myServerAddress"); // Compliant, inside we only look at 'Password=;'
            builder.UseMySQL("Password=password"); // compliant, not empty

            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=False;Password="); // Noncompliant
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=false;Password="); // Noncompliant
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Trusted_Connection=no;Password="); // Noncompliant
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Trusted_Connection=foo;Password="); // Noncompliant
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=maybe;Password="); // Noncompliant
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
            builder.UseSqlServer("x" + "y" + "Password=;"); // Noncompliant
            builder.UseOracle("x" + "y" + "Password=;" + "z"); // Noncompliant
            builder.UseSqlServer("x" + "y" + "a;Password=;y" + "z"); // Noncompliant
            builder.UseSqlServer("x" + "y" + "Password=" + a);
            builder.UseSqlServer("x" + "y" + "Password=" + ""); // FN, edge case
            builder.UseSqlServer("x" + "y" + "Password=" + ";"); // FN, edge case
            builder.UseSqlite("x" + "y" + "Password=;" + a); // Noncompliant
            builder.UseNpgsql($"Server={a};Database={a};User Id={a};Password="); // Noncompliant
            builder.UseOracle($"Server={a};Database={a};User Id={a};Password=;Something={a}"); // Noncompliant
            builder.UseSqlite($"Server={a};Database={a}" + $";User Id={a};Password=;Foo" + $"Something={a}"); // Noncompliant

            builder.UseNpgsql($"Server={a};Database={a};User Id={a};Password={a}"); // compliant
            builder.UseNpgsql($"Server={a};Database={a};User Id={a};Password=" + a); // compliant

            // This FP is tolerated in order to keep the implementation simple
            builder.UseSqlServer("Password=\""); // Noncompliant FP
            builder.UseSqlServer("Password=\"mypassword\""); // compliant, not empty
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

        public void Compliant(DbContextOptionsBuilder optionsBuilder, string password, DbConnection dbConnection)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/connection-string-syntax#windows-authentication
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=SSPI");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=True");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Trusted_Connection=true");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Trusted_Connection=yes");

            optionsBuilder.UseSqlServer("Password=Foo"); // not empty
            optionsBuilder.UseSqlServer("Password = "); // FN, has spaces

            // Integrated Security overrides, not vulnerable.
            // We don't actually map the correct parameters to the provider, we keep things simple (and may have FNs)
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=SSPI;Password=");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=true;Password=;");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated SECURITY=TRUE;Password=;");
            optionsBuilder.UseMySQL("Server=myServerAddress;Database=myDataBase;Integrated Security=yes;Password=;"); // FN, "yes" is for OracleClient
            optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;Trusted_Connection=yes;Password=;");
            optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;TRUSTED_CONNECTION=YES;Password=;");
            optionsBuilder.UseSqlite("Server=myServerAddress;Database=myDataBase;Password=;Trusted_Connection=yes");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Trusted_Connection=True");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Trusted Connection=True");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Integrated Security=true");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Trusted Connection=Yes");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Integrated Security=yes");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Integrated Security=SSPI");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Trusted_Connection=True");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Integrated_Security=true");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Trusted_Connection=Yes");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Integrated_Security=yes");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Password=;Integrated_Security=sspi");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword");

            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Integrated Security=False;Password=123");
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;Trusted_Connection=no;Password=123");

            optionsBuilder.UseSqlite(dbConnection);
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

    public class CustomCodeSameName : Microsoft.EntityFrameworkCore.DbContext
    {
        Foo foo;
        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            optionsBuilder.UseMySQL("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
            Foo("Password=");
            Foo("Password=;");
            Foo($"Password=;");
            Foo("a" + "Password=;" + "a");

            foo.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=");
        }

        void Foo(string s) { }
    }
}
