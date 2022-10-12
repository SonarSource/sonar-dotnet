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
    }
}

class Entity { }
