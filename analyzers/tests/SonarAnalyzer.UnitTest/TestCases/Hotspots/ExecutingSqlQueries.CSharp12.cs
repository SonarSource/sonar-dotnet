using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

class ClassWithPrimaryConstructor(DbContext context, SqliteConnection connection, string someUserInput, params object[] parameters)
{
    void Foo()
    {
        context.Query<object>().FromSql(someUserInput, parameters); // Compliant, we don't know anything about the someUserInput parameter
        SqliteCommand command = new($"SELECT * FROM mytable WHERE mycol={someUserInput}", connection); // Noncompliant
    }
}
