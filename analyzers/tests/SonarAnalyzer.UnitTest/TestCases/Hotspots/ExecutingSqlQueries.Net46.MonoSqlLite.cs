using System;
using System.Linq;
using Mono.Data.Sqlite;

namespace Test
{
    public class C
    {
        string ConstQuery = "";

        void Compliant(SqliteConnection connection)
        {
            var command = new SqliteCommand();       // Compliant
            command = new SqliteCommand(connection); // Compliant
            var adapter = new SqliteDataAdapter();   // Compliant
        }

        void Foo(SqliteConnection connection, string query, params object[] parameters)
        {
            var command = new SqliteCommand($"SELECT * FROM mytable WHERE mycol={query}", connection);  // Noncompliant
            command = new SqliteCommand($"SELECT * FROM mytable WHERE mycol={query}");                  // Noncompliant
            var adapter = new SqliteDataAdapter(string.Concat(query, parameters), connection);          // Noncompliant
        }
    }
}
