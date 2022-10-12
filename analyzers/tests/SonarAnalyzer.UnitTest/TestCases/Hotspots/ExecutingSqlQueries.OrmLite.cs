using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

class Program
{
    public async Task OrmLiteReadApiMethods(IDbConnection dbConn, string query, string param)
    {
        dbConn.Select<Entity>(query);                                                         // Compliant
        dbConn.Select<Entity>(query + param);                                                 // Noncompliant
        dbConn.Select<Entity>(typeof(Program), query + param, new { a = 1 });                 // Noncompliant
        OrmLiteReadApi.Select<Entity>(dbConn, typeof(Program), query + param, new { a = 1 }); // FN. string argument is in the thrid position if this overload is called in the unreduced form.

        await dbConn.SelectAsync<Entity>(query);                                                              // Compliant
        await dbConn.SelectAsync<Entity>(query + param);                                                      // Noncompliant
        await dbConn.SelectAsync<Entity>(typeof(Program), query + param, new { a = 1 });                      // Noncompliant
        await OrmLiteReadApiAsync.SelectAsync<Entity>(dbConn, typeof(Program), query + param, new { a = 1 }); // FN. string argument is in the thrid position if this overload is called in the unreduced form.

        dbConn.SelectLazy<Entity>(query);                         // Compliant
        dbConn.SelectLazy<Entity>(query + param);                 // Noncompliant
        OrmLiteReadApi.SelectLazy<Entity>(dbConn, query + param); // Noncompliant

        dbConn.SelectNonDefaults(query, new Entity { Age = 42 });                         // Compliant
        dbConn.SelectNonDefaults(query + param, new Entity { Age = 42 });                 // Noncompliant
        OrmLiteReadApi.SelectNonDefaults(dbConn, query + param, new Entity { Age = 42 }); // Noncompliant

        await dbConn.SelectNonDefaultsAsync(query, new Entity { Age = 42 });                         // Compliant
        await dbConn.SelectNonDefaultsAsync(query + param, new Entity { Age = 42 });                 // Noncompliant
        await OrmLiteReadApiAsync.SelectNonDefaultsAsync(dbConn, query + param, new Entity { Age = 42 }); // Noncompliant

        dbConn.Single<Entity>(query);                                           // Compliant
        dbConn.Single<Entity>(query + param);                                   // Noncompliant
        OrmLiteReadApi.Single<Entity>(dbConn, query + param, new { Age = 42 }); // Noncompliant
    }
}

class Entity
{
    public int Age { get; set; }
}
