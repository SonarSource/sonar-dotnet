using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Tests.Diagnostics
{
    class MyClass(DbContext context, SqliteConnection connection, string someUserInput, params object[] parameters)
    {
        void MyMethod()
        {
            context.Query<object>().FromSql(someUserInput, parameters); // Compliant FN
            SqliteCommand command = new($"""SELECT * FROM mytable WHERE mycol={someUserInput}""", connection); // Noncompliant
        }
    }
}
