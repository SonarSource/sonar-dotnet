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
            context.Database.ExecuteSqlCommand($"", parameters); // Noncompliant FP, interpolated string with argument tranformed in RawQuery
            context.Database.ExecuteSqlCommand(query, parameters); // Compliant, not concat or format
            context.Database.ExecuteSqlCommand("" + query, parameters); // Noncompliant

            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query} AND mycol2={0}", parameters[0]); // Noncompliant, string interpolation  it is RawSqlString
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant, RawSqlQuery
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}"); // Compliant, FormattableString is sanitized

            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, query); // Compliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, $"SELECT * FROM mytable WHERE mycol={query}{query}", x, guid); // Noncompliant

            context.Database.ExecuteSqlCommandAsync($""); // Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync(""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(ConstQuery); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" + ""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(query); // Compliant, not concat
            context.Database.ExecuteSqlCommandAsync("" + query); // Noncompliant
            context.Database.ExecuteSqlCommandAsync($"", parameters); // Noncompliant FP, interpolated string with argument tranformed in RawQuery
            context.Database.ExecuteSqlCommandAsync(query, parameters); // Compliant, not concat or format
            context.Database.ExecuteSqlCommandAsync("" + query, parameters); // Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommandAsync(context.Database, "" + query, parameters);  // Noncompliant

            context.Query<User>().FromSql($""); // Compliant, FormattableString is sanitized
            context.Query<User>().FromSql(""); // Compliant, constants are safe
            context.Query<User>().FromSql(ConstQuery); // Compliant, constants are safe
            context.Query<User>().FromSql(query); // Compliant, not concat/format
            context.Query<User>().FromSql("" + ""); // Compliant
            context.Query<User>().FromSql($"", parameters); // Noncompliant FP, interpolated string with argument tranformed in RawQuery
            context.Query<User>().FromSql("", parameters); // Compliant, the parameters are sanitized
            context.Query<User>().FromSql(query, parameters); // Compliant
            context.Query<User>().FromSql("" + query, parameters); // Noncompliant
            RelationalQueryableExtensions.FromSql(context.Query<User>(), "" + query, parameters); // Noncompliant
        }

        public void ConcatAndFormat(DbContext context, string query, params object[] parameters)
        {
            context.Database.ExecuteSqlCommand(string.Concat(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommand(string.Format(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommand(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Compliant, the FormattableString is transformed into a parametrized query.
            context.Database.ExecuteSqlCommand("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            var formatted = string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters);
            context.Database.ExecuteSqlCommand(formatted); // FN

            context.Database.ExecuteSqlCommandAsync(string.Concat(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommandAsync(string.Format(query, parameters)); // Noncompliant
            context.Database.ExecuteSqlCommandAsync(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Database.ExecuteSqlCommandAsync($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Compliant, the FormattableString is transformed into a parametrized query.
            context.Database.ExecuteSqlCommandAsync("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            var concatenated = string.Concat(query, parameters);
            context.Database.ExecuteSqlCommandAsync(concatenated); // FN

            context.Query<User>().FromSql(string.Concat(query, parameters)); // Noncompliant
            context.Query<User>().FromSql(string.Format("INSERT INTO Users (name) VALUES (\"{0}\")", parameters)); // Noncompliant
            context.Query<User>().FromSql($"SELECT * FROM mytable WHERE mycol={parameters[0]}"); // Compliant, the FormattableString is transformed into a parametrized query.
            context.Query<User>().FromSql("SELECT * FROM mytable WHERE mycol=" + parameters[0]); // Noncompliant
            var interpolated = $"SELECT * FROM mytable WHERE mycol={parameters[0]}";
            context.Query<User>().FromSql(interpolated); // FN
        }
    }

    class User
    {
        string Id { get; set; }
        string Name { get; set; }
    }
}
