using System;
using Microsoft.EntityFrameworkCore;

namespace Tests.Diagnostics
{
    class Program
    {
        private const string ConstQuery = "";

        public void Foo(DbContext context, string query, int x, Guid guid, params object[] parameters)
        {
            context.Database.ExecuteSqlRaw($""); // Compliant
            context.Database.ExecuteSqlRaw(""); // Compliant, constants are safe
            context.Database.ExecuteSqlRaw(ConstQuery); // Compliant, constants are safe
            context.Database.ExecuteSqlRaw("" + ""); // Compliant, constants are safe
            context.Database.ExecuteSqlRaw(query); // Compliant, not concat or format
            context.Database.ExecuteSqlRaw("" + query); // Noncompliant
            context.Database.ExecuteSqlRaw($"", parameters); // Compliant
            context.Database.ExecuteSqlRaw(query, parameters); // Compliant, not concat or format

            context.Database.ExecuteSqlRaw("" + query, parameters); // Noncompliant
            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={query} AND mycol2={0}", parameters[0]); // Noncompliant
            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant
            context.Database.ExecuteSqlRaw(@$"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant
            context.Database.ExecuteSqlRaw($@"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant
            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={query}"); // Noncompliant

            RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(context.Database, query); // Compliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(context.Database, $"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant

            context.Database.ExecuteSqlRawAsync($""); // Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync(""); // Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync(ConstQuery); // Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync("" + ""); // Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync(query); // Compliant, not concat
            context.Database.ExecuteSqlRawAsync("" + query); // Noncompliant
            context.Database.ExecuteSqlRawAsync(query + ""); // Noncompliant
            context.Database.ExecuteSqlRawAsync("" + query + ""); // Noncompliant
            context.Database.ExecuteSqlRawAsync($"", parameters); // Compliant
            context.Database.ExecuteSqlRawAsync(query, parameters); // Compliant, not concat or format

            context.Database.ExecuteSqlRawAsync("" + query, parameters); // Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "" + query, parameters);  // Noncompliant

            context.Set<User>().FromSqlRaw($""); // Compliant
            context.Set<User>().FromSqlRaw(""); // Compliant, constants are safe
            context.Set<User>().FromSqlRaw(ConstQuery); // Compliant, constants are safe
            context.Set<User>().FromSqlRaw(query); // Compliant, not concat/format
            context.Set<User>().FromSqlRaw("" + ""); // Compliant
            context.Set<User>().FromSqlRaw($"", parameters); // Compliant
            context.Set<User>().FromSqlRaw("", parameters); // Compliant, the parameters are sanitized
            context.Set<User>().FromSqlRaw(query, parameters); // Compliant
            context.Set<User>().FromSqlRaw("" + query, parameters); // Noncompliant
            RelationalQueryableExtensions.FromSqlRaw(context.Set<User>(), "" + query, parameters); // Noncompliant
        }

        public void ConcatAndFormat(DbContext context, string query, params object[] parameters)
        {
            var concatenated = string.Concat(query, parameters);                                    // Secondary [1,2,3]
            var formatted = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters); // Secondary [4,5,6]
            var interpolated = $"SELECT * FROM mytable WHERE mycol={parameters[0]}";                // Secondary [7,8,9]

            context.Database.ExecuteSqlRaw(string.Concat(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlRaw(string.Format(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlRaw(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Noncompliant
            context.Database.ExecuteSqlRaw("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            context.Database.ExecuteSqlRaw(formatted);    // Noncompliant [4]
            context.Database.ExecuteSqlRaw(concatenated); // Noncompliant [1]
            context.Database.ExecuteSqlRaw(interpolated); // Noncompliant [7]

            context.Database.ExecuteSqlRawAsync(string.Concat(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlRawAsync(string.Format(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlRawAsync(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Database.ExecuteSqlRawAsync($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Noncompliant
            context.Database.ExecuteSqlRawAsync("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            context.Database.ExecuteSqlRawAsync(formatted);    // Noncompliant [5]
            context.Database.ExecuteSqlRawAsync(concatenated); // Noncompliant [2]
            context.Database.ExecuteSqlRawAsync(interpolated); // Noncompliant [8]

            context.Set<User>().FromSqlRaw(string.Concat(query, parameters)); // Noncompliant
            context.Set<User>().FromSqlRaw(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Set<User>().FromSqlRaw($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Noncompliant
            context.Set<User>().FromSqlRaw("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            context.Set<User>().FromSqlRaw(formatted);    // Noncompliant [6]
            context.Set<User>().FromSqlRaw(concatenated); // Noncompliant [3]
            context.Set<User>().FromSqlRaw(interpolated); // Noncompliant [9]
        }

        public void Foo(BloggingContext context, string query)
        {
            var a = context.Blogs.FromSqlRaw($"{query}"); // Noncompliant
            var b = context.Blogs.FromSqlInterpolated($"{query}"); // Compliant, FromSqlInterpolated is safe https://learn.microsoft.com/ef/core/querying/sql-queries#passing-parameters
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

            context.Database.ExecuteSqlRaw(query); // Compliant
            context.Database.ExecuteSqlRaw(query2); // Noncompliant [c1] - FP
            context.Database.ExecuteSqlRaw($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Noncompliant - FP

            context.Database.ExecuteSqlRawAsync(query); // Compliant
            context.Database.ExecuteSqlRawAsync(query2); // Noncompliant [c2] - FP
            context.Database.ExecuteSqlRawAsync($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Noncompliant - FP

            context.Set<User>().FromSqlRaw(query); // Compliant
            context.Set<User>().FromSqlRaw(query2); // Noncompliant [c3] - FP
            context.Set<User>().FromSqlRaw($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Noncompliant - FP
        }
    }
}
