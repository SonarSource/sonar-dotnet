using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

const string part1 = """SELECT * FROM""";
const string part2 = """ mytable WHERE mycol=""";
const string constQuery = $"""{part1}{part2}""";

void RawStringLiterals(DbContext context, SqliteConnection connection, string someUserInput, params object[] parameters)
{
    context.Query<User>().FromSql(constQuery, parameters); // Compliant
    SqliteCommand command = new($"""SELECT * FROM mytable WHERE mycol={someUserInput}""", connection);  // Noncompliant
}

void NewlinesInStringInterpolation(SqliteConnection connection, string someUserInput)
{
    SqliteCommand command = new($"SELECT * FROM mytable WHERE mycol={someUserInput // Noncompliant
        .ToLower()}", connection);
    SqliteCommand commandRawString = new($$"""SELECT * FROM mytable WHERE mycol={{someUserInput // Noncompliant
        .ToLower()}}""", connection);
}

record User
{
    string Id { get; set; }
    string Name { get; set; }
}
