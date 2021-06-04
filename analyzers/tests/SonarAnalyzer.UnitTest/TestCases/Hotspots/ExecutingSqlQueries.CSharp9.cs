using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

string ConstQuery = "";

void Foo(DbContext context, SqliteConnection connection, string query, params object[] parameters)
{
    context.Database.ExecuteSqlCommand(ConstQuery); // Compliant, constants are safe
    context.Database.ExecuteSqlCommand(query); // Compliant, not concat or format

    context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query} AND mycol2={0}", parameters[0]); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}

    context.Query<User>().FromSql(ConstQuery);                                               // Compliant, constants are safe
    context.Query<User>().FromSql("" + query, parameters);                                   // Noncompliant
    RelationalQueryableExtensions.FromSql(context.Query<User>(), "" + query, parameters);    // Noncompliant

    SqliteCommand command = new ($"SELECT * FROM mytable WHERE mycol={query}", connection);  // Noncompliant
}

record User
{
    string Id { get; set; }
    string Name { get; set; }
}
