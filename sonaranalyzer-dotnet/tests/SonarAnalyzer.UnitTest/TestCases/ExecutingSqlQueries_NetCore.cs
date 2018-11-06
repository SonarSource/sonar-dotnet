using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Tests.Diagnostics
{
    class Program
    {
        private const string ConstQuery = "";

        public void Foo(DbContext context, string query, params object[] parameters)
        {
            context.Database.ExecuteSqlCommand($""); // Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommand(""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommand(ConstQuery); // Compliant, constants are safe
            context.Database.ExecuteSqlCommand("" + ""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommand(query); // Noncompliant
            context.Database.ExecuteSqlCommand("" + query); // Noncompliant
            context.Database.ExecuteSqlCommand($"", parameters); // Noncompliant, the FormattableString is evaluated before and is not sanitized
            context.Database.ExecuteSqlCommand(query, parameters); // Noncompliant
            context.Database.ExecuteSqlCommand("" + query, parameters); // Noncompliant

            context.Database.ExecuteSqlCommandAsync($""); // Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync(""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(ConstQuery); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" + ""); // Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(query); // Noncompliant
            context.Database.ExecuteSqlCommandAsync("" + query); // Noncompliant
            context.Database.ExecuteSqlCommandAsync($"", parameters); // Noncompliant, the FormattableString is evaluated before and is not sanitized
            context.Database.ExecuteSqlCommandAsync(query, parameters); // Noncompliant
            context.Database.ExecuteSqlCommandAsync("" + query, parameters); // Noncompliant

            context.Query<User>().FromSql($""); // Compliant, FormattableString is sanitized
            context.Query<User>().FromSql(""); // Compliant, constants are safe
            context.Query<User>().FromSql(ConstQuery); // Compliant, constants are safe
            context.Query<User>().FromSql(query); // Noncompliant
            context.Query<User>().FromSql("" + ""); // Compliant
            context.Query<User>().FromSql($"", parameters); // Noncompliant, the FormattableString is evaluated before and is not sanitized
            context.Query<User>().FromSql("", parameters); // Compliant, the parameters are sanitized
            context.Query<User>().FromSql(query, parameters); // Noncompliant, even though the parameters are sanitized, query could be tainted
            context.Query<User>().FromSql("" + query, parameters); // Noncompliant
        }
    }

    class User
    {
        string Id { get; set; }
        string Name { get; set; }
    }
}
