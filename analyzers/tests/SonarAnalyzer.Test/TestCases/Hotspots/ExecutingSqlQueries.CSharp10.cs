using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

const string part1 = "SELECT * FROM";
const string part2 = " mytable WHERE mycol=";
const string querry = $"{part1}{part2}";

void Foo(DbContext context, SqliteConnection connection, params object[] parameters)
{
    context.Query<User>().FromSql("" + querry, parameters); // Compliant
}


record User
{
    string Id { get; set; }
    string Name { get; set; }
}
