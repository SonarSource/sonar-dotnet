using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    public async Task DatabaseMethods(Database database, string query, int x)
    {
        database.SqlQuery<Program>(query);     // Compliant
        database.SqlQuery<Program>(query + x); // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
        database.SqlQuery(typeof(Program), query + x);                                             // Noncompliant
        database.SqlQuery<Program>($"{query} {x}");                                                // Noncompliant
        database.SqlQuery<Program>(string.Format("Select * from Program Where Id={1}", x));        // Noncompliant

        database.ExecuteSqlCommand(query);                                                         // Compliant
        database.ExecuteSqlCommand(query + x);                                                     // Noncompliant
        database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, query + x);            // Noncompliant
        await database.ExecuteSqlCommandAsync(query);                                              // Compliant
        await database.ExecuteSqlCommandAsync(query + x);                                          // Noncompliant
        await database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, query + x); // Noncompliant
    }

    public void DbSetMethods(DbSet set, string query, int x, int param)
    {
        set.SqlQuery(query);            // Compliant
        set.SqlQuery(query + x);        // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
        set.SqlQuery(query, param);     // Compliant
        set.SqlQuery(query + x, param); // Noncompliant
    }

    public void DbSetMethods(DbSet<Program> set, string query, int x, int param)
    {
        set.SqlQuery(query);            // Compliant
        set.SqlQuery(query + x);        // Noncompliant {{Make sure using a dynamically formatted SQL query is safe here.}}
        set.SqlQuery(query, param);     // Compliant
        set.SqlQuery(query + x, param); // Noncompliant
    }
}
