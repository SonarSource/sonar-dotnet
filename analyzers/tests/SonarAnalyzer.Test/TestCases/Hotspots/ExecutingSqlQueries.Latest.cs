using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

string ConstQuery = "";

const string part1 = "SELECT * FROM";
const string part2 = " mytable WHERE mycol=";
const string query = $"{part1}{part2}";

const string part3 = """SELECT * FROM""";
const string part4 = """ mytable WHERE mycol=""";
const string rawConstQuery = $"""{part3}{part4}""";

void Foo(DbContext context, SqliteConnection connection, string myQuery, params object[] parameters)
{
    context.Database.ExecuteSqlCommand(ConstQuery); // Compliant, constants are safe
    context.Database.ExecuteSqlCommand(myQuery); // Compliant, not concat or format

    context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={myQuery} AND mycol2={0}", parameters[0]); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}

    context.Query<User>().FromSql(ConstQuery);                                               // Compliant, constants are safe
    context.Query<User>().FromSql("" + myQuery, parameters);                                   // Noncompliant
    RelationalQueryableExtensions.FromSql(context.Query<User>(), "" + myQuery, parameters);    // Noncompliant

    SqliteCommand command = new($"SELECT * FROM mytable WHERE mycol={myQuery}", connection);  // Noncompliant
}

void Foo2(DbContext context, SqliteConnection connection, string notConstant, params object[] parameters)
{
    context.Query<User>().FromSql("" + query, parameters);         // Compliant
    context.Query<User>().FromSql("" + notConstant, parameters);    // Noncompliant
}

void RawStringLiterals(DbContext context, SqliteConnection connection, string someUserInput, params object[] parameters)
{
    context.Query<User>().FromSql(rawConstQuery, parameters); // Compliant
    SqliteCommand command = new($"""SELECT * FROM mytable WHERE mycol={someUserInput}""", connection);  // Noncompliant
}

void NewlinesInStringInterpolation(SqliteConnection connection, string someUserInput)
{
    SqliteCommand command = new($"SELECT * FROM mytable WHERE mycol={someUserInput // Noncompliant
        .ToLower()}", connection);
    SqliteCommand commandRawString = new($$"""SELECT * FROM mytable WHERE mycol={{someUserInput // Noncompliant
        .ToLower()}}""", connection);
}

class ClassWithPrimaryConstructor(DbContext context, SqliteConnection connection, string someUserInput, params object[] parameters)
{
    void Foo()
    {
        context.Query<object>().FromSql(someUserInput, parameters); // Compliant, we don't know anything about the someUserInput parameter
        SqliteCommand command = new($"SELECT * FROM mytable WHERE mycol={someUserInput}", connection); // Noncompliant
    }
}

partial class Partial
{
    partial string PropertyQuery => "SELECT * FROM mytable WHERE mycol=";
}

partial class Partial
{
    partial string PropertyQuery { get; }

    void EscapeSequence(DbContext context, SqliteConnection connection, string myQuery, params object[] parameters)
    {
        const string constQuery = "SELECT * FROM mytable\eWHERE mycol=";
        context.Query<User>().FromSql("\e" + myQuery, parameters);       // Noncompliant
        context.Query<User>().FromSql("\e" + constQuery, parameters);    // Compliant: constants are safe
        context.Query<User>().FromSql("\e" + PropertyQuery, parameters); // Noncompliant
        context.Query<User>().FromSql("\e" + PropertyQuery, "\e");       // Noncompliant
    }
}

record User
{
    string Id { get; set; }
    string Name { get; set; }
}
