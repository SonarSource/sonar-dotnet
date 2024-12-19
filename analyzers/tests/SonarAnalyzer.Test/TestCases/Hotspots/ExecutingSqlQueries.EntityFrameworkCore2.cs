using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Tests.Diagnostics
{
    class Program
    {
        private const string ConstQuery = "";

        public void Foo(DbContext context, string query, int x, Guid guid, params object[] parameters)
        {
            context.Database.ExecuteSqlCommand($""); // Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommand(""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommand(ConstQuery); // Compliant, constants are safe
            context.Database.ExecuteSqlCommand("" + ""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommand(query); // Compliant, not concat or format
            context.Database.ExecuteSqlCommand("" + query); // Noncompliant
            context.Database.ExecuteSqlCommand($"", parameters); // Compliant. Was FP until Roslyn 3.11.0. Interpolated string with argument tranformed in RawQuery
            context.Database.ExecuteSqlCommand(query, parameters); // Compliant, not concat or format
            context.Database.ExecuteSqlCommand("" + query, parameters); // Noncompliant

            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query} AND mycol2={0}", parameters[0]); // Noncompliant, string interpolation  it is RawSqlString
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant, RawSqlQuery
            context.Database.ExecuteSqlCommand(@$"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant, RawSqlQuery
            context.Database.ExecuteSqlCommand($@"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant, RawSqlQuery
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}"); // Compliant, FormattableString is sanitized

            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, query); // Compliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, $"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant

            context.Database.ExecuteSqlCommandAsync($""); // Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync(""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(ConstQuery); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" + ""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(query); // Compliant, not concat
            context.Database.ExecuteSqlCommandAsync("" + query); // Noncompliant
            context.Database.ExecuteSqlCommandAsync(query + ""); // Noncompliant
            context.Database.ExecuteSqlCommandAsync("" + query + ""); // Noncompliant
            context.Database.ExecuteSqlCommandAsync($"", parameters); // Compliant. Was FP until Roslyn 3.11.0. Interpolated string with argument tranformed in RawQuery
            context.Database.ExecuteSqlCommandAsync(query, parameters); // Compliant, not concat or format
            context.Database.ExecuteSqlCommandAsync("" + query, parameters); // Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommandAsync(context.Database, "" + query, parameters);  // Noncompliant

            context.Query<User>().FromSql($""); // Compliant, FormattableString is sanitized
            context.Query<User>().FromSql(""); // Compliant, constants are safe
            context.Query<User>().FromSql(ConstQuery); // Compliant, constants are safe
            context.Query<User>().FromSql(query); // Compliant, not concat/format
            context.Query<User>().FromSql("" + ""); // Compliant
            context.Query<User>().FromSql($"", parameters); // Compliant. Was FP until Roslyn 3.11.0. Interpolated string with argument tranformed in RawQuery
            context.Query<User>().FromSql("", parameters); // Compliant, the parameters are sanitized
            context.Query<User>().FromSql(query, parameters); // Compliant
            context.Query<User>().FromSql("" + query, parameters); // Noncompliant
            RelationalQueryableExtensions.FromSql(context.Query<User>(), "" + query, parameters); // Noncompliant
        }

        public void ConcatAndFormat(DbContext context, string query, params object[] parameters)
        {
            var concatenated = string.Concat(query, parameters);                                    // Secondary [1,2,3]
            var formatted = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters); // Secondary [4,5,6]
            var interpolated = $"SELECT * FROM mytable WHERE mycol={parameters[0]}";                // Secondary [7,8,9]

            context.Database.ExecuteSqlCommand(string.Concat(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommand(string.Format(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Compliant, the FormattableString is transformed into a parametrized query.
            context.Database.ExecuteSqlCommand("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            context.Database.ExecuteSqlCommand(formatted);    // Noncompliant [4]
            context.Database.ExecuteSqlCommand(concatenated); // Noncompliant [1]
            context.Database.ExecuteSqlCommand(interpolated); // Noncompliant [7]

            context.Database.ExecuteSqlCommandAsync(string.Concat(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommandAsync(string.Format(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommandAsync(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Database.ExecuteSqlCommandAsync($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Compliant, the FormattableString is transformed into a parametrized query.
            context.Database.ExecuteSqlCommandAsync("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            context.Database.ExecuteSqlCommandAsync(formatted);    // Noncompliant [5]
            context.Database.ExecuteSqlCommandAsync(concatenated); // Noncompliant [2]
            context.Database.ExecuteSqlCommandAsync(interpolated); // Noncompliant [8]

            context.Query<User>().FromSql(string.Concat(query, parameters)); // Noncompliant
            context.Query<User>().FromSql(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Query<User>().FromSql($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Compliant, the FormattableString is transformed into a parametrized query.
            context.Query<User>().FromSql("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            context.Query<User>().FromSql(formatted);    // Noncompliant [6]
            context.Query<User>().FromSql(concatenated); // Noncompliant [3]
            context.Query<User>().FromSql(interpolated); // Noncompliant [9]
        }

        public void Foo(BloggingContext context, string query)
        {
            var b = context.Blogs.FromSql($"{query}"); // Compliant: interpolated strings are safe in EF (https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.relationalqueryableextensions.fromsql?view=efcore-2.0#microsoft-entityframeworkcore-relationalqueryableextensions-fromsql-1(system-linq-iqueryable((-0))-system-formattablestring))
        }
    }

    class User
    {
        string Id { get; set; }
        string Name { get; set; }
    }

    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
    }

    public class Blog
    {
        public string Url { get; set; }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9602
    class Repro_9602
    {
        public void ConstantQuery(DbContext context, bool onlyEnabled)
        {
            string query = "SELECT id FROM users";
            if(onlyEnabled)
                query += " WHERE enabled = 1";
            string query2 = $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"; // Secondary [c1, c2, c3]

            context.Database.ExecuteSqlCommand(query); // Compliant
            context.Database.ExecuteSqlCommand(query2); // Noncompliant [c1] - FP
            context.Database.ExecuteSqlCommand($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Compliant

            context.Database.ExecuteSqlCommandAsync(query); // Compliant
            context.Database.ExecuteSqlCommandAsync(query2); // Noncompliant [c2] - FP
            context.Database.ExecuteSqlCommandAsync($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Compliant

            context.Query<User>().FromSql(query); // Compliant
            context.Query<User>().FromSql(query2); // Noncompliant [c3] - FP
            context.Query<User>().FromSql($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Compliant
        }
    }
}
