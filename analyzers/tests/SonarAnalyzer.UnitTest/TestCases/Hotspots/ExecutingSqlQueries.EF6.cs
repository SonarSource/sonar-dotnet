using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    class Program
    {
        public async Task DatabaseMethods(Database database, string query, int x)
        {
            database.SqlQuery<Program>(query + x); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
            database.SqlQuery(typeof(Program), query + x);                                             // Noncompliant
            database.SqlQuery<Program>($"{query} {x}");                                                // Noncompliant
            database.SqlQuery<Program>(string.Format("Select * from Program Where Id={1}", x));        // Noncompliant

            database.ExecuteSqlCommand(query + x);                                                     // Noncompliant
            database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, query + x);            // Noncompliant
            await database.ExecuteSqlCommandAsync(query + x);                                          // Noncompliant
            await database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, query + x); // Noncompliant
        }

        public async Task DbSetMethods(DbSet set, string query, int x)
        {
            set.SqlQuery(query + x);    // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
            set.SqlQuery(query + x, x); // Noncompliant
        }

        public async Task DbSetMethods(DbSet<Program> set, string query, int x)
        {
            set.SqlQuery(query + x);    // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
            set.SqlQuery(query + x, x); // Noncompliant
        }
    }
}
