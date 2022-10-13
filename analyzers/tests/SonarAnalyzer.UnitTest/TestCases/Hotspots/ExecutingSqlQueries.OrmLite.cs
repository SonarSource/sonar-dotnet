using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

class Program
{
    public void OrmLiteReadApiMethods(IDbConnection dbConn, string query, string param)
    {
        dbConn.Select<Entity>(query);                                                                         // Compliant
        dbConn.Select<Entity>(query + param);                                                                 // Noncompliant
        dbConn.Select<Entity>(typeof(Program), query + param, new { a = 1 });                                 // Noncompliant
        OrmLiteReadApi.Select<Entity>(dbConn, typeof(Program), query + param, new { a = 1 });                 // FN. string argument is in the thrid position if this overload is called in the unreduced form.

        dbConn.SelectLazy<Entity>(query);                                                                     // Compliant
        dbConn.SelectLazy<Entity>(query + param);                                                             // Noncompliant
        OrmLiteReadApi.SelectLazy<Entity>(dbConn, query + param);                                             // Noncompliant

        dbConn.SelectNonDefaults(query, new Entity { Age = 42 });                                             // Compliant
        dbConn.SelectNonDefaults(query + param, new Entity { Age = 42 });                                     // Noncompliant
        OrmLiteReadApi.SelectNonDefaults(dbConn, query + param, new Entity { Age = 42 });                     // Noncompliant

        dbConn.Single<Entity>(query);                                                                         // Compliant
        dbConn.Single<Entity>(query + param);                                                                 // Noncompliant
        OrmLiteReadApi.Single<Entity>(dbConn, query + param, new { Age = 42 });                               // Noncompliant

        dbConn.Scalar<Entity>(query);                                                                         // Compliant
        dbConn.Scalar<Entity>(query + param);                                                                 // Noncompliant
        OrmLiteReadApi.Scalar<Entity>(dbConn, query + param, new { Age = 42 });                               // Noncompliant

        dbConn.Column<Entity>(query);                                                                         // Compliant
        dbConn.Column<Entity>(query + param);                                                                 // Noncompliant
        OrmLiteReadApi.Column<Entity>(dbConn, query + param, new { Age = 42 });                               // Noncompliant

        dbConn.ColumnLazy<Entity>(query);                                                                     // Compliant
        dbConn.ColumnLazy<Entity>(query + param);                                                             // Noncompliant
        OrmLiteReadApi.ColumnLazy<Entity>(dbConn, query + param, new { Age = 42 });                           // Noncompliant

        dbConn.ColumnDistinct<Entity>(query);                                                                 // Compliant
        dbConn.ColumnDistinct<Entity>(query + param);                                                         // Noncompliant
        OrmLiteReadApi.ColumnDistinct<Entity>(dbConn, query + param, new { Age = 42 });                       // Noncompliant

        dbConn.Lookup<string, Entity>(query);                                                                 // Compliant
        dbConn.Lookup<string, Entity>(query + param);                                                         // Noncompliant
        OrmLiteReadApi.Lookup<string, Entity>(dbConn, query + param, new { Age = 42 });                       // Noncompliant

        dbConn.Dictionary<string, Entity>(query);                                                             // Compliant
        dbConn.Dictionary<string, Entity>(query + param);                                                     // Noncompliant
        OrmLiteReadApi.Dictionary<string, Entity>(dbConn, query + param, new { Age = 42 });                   // Noncompliant

        dbConn.Exists<Entity>(query);                                                                         // Compliant
        dbConn.Exists<Entity>(query + param);                                                                 // Noncompliant
        OrmLiteReadApi.Exists<Entity>(dbConn, query + param, new { Age = 42 });                               // Noncompliant

        dbConn.SqlList<Entity>(query);                                                                        // Compliant
        dbConn.SqlList<Entity>(query + param);                                                                // Noncompliant
        OrmLiteReadApi.SqlList<Entity>(dbConn, query + param, new { Age = 42 });                              // Noncompliant

        dbConn.SqlColumn<Entity>(query);                                                                      // Compliant
        dbConn.SqlColumn<Entity>(query + param);                                                              // Noncompliant
        OrmLiteReadApi.SqlColumn<Entity>(dbConn, query + param, new { Age = 42 });                            // Noncompliant

        dbConn.SqlScalar<Entity>(query);                                                                      // Compliant
        dbConn.SqlScalar<Entity>(query + param);                                                              // Noncompliant
        OrmLiteReadApi.SqlScalar<Entity>(dbConn, query + param, new { Age = 42 });                            // Noncompliant

        dbConn.ExecuteNonQuery(query);                                                                        // Compliant
        dbConn.ExecuteNonQuery(query + param);                                                                // Noncompliant
        OrmLiteReadApi.ExecuteNonQuery(dbConn, query + param, new { Age = 42 });                              // Noncompliant
    }

    public async Task OrmLiteReadApiAsyncMethods(IDbConnection dbConn, string query, string param)
    {
        await dbConn.SelectAsync<Entity>(query);                                                              // Compliant
        await dbConn.SelectAsync<Entity>(query + param);                                                      // Noncompliant
        await dbConn.SelectAsync<Entity>(typeof(Program), query + param, new { a = 1 });                      // Noncompliant
        await OrmLiteReadApiAsync.SelectAsync<Entity>(dbConn, typeof(Program), query + param, new { a = 1 }); // FN. string argument is in the thrid position if this overload is called in the unreduced form.

        await dbConn.SelectNonDefaultsAsync(query, new Entity { Age = 42 });                                  // Compliant
        await dbConn.SelectNonDefaultsAsync(query + param, new Entity { Age = 42 });                          // Noncompliant
        await OrmLiteReadApiAsync.SelectNonDefaultsAsync(dbConn, query + param, new Entity { Age = 42 });     // Noncompliant

        await dbConn.SingleAsync<Entity>(query);                                                              // Compliant
        await dbConn.SingleAsync<Entity>(query + param);                                                      // Noncompliant
        await OrmLiteReadApiAsync.SingleAsync<Entity>(dbConn, query + param, new { Age = 42 });               // Noncompliant

        await dbConn.ScalarAsync<Entity>(query);                                                              // Compliant
        await dbConn.ScalarAsync<Entity>(query + param);                                                      // Noncompliant
        OrmLiteReadApiAsync.ScalarAsync<Entity>(dbConn, query + param, new { Age = 42 });                     // Noncompliant

        await dbConn.ColumnAsync<Entity>(query);                                                              // Compliant
        await dbConn.ColumnAsync<Entity>(query + param);                                                      // Noncompliant
        OrmLiteReadApiAsync.ColumnAsync<Entity>(dbConn, query + param, new { Age = 42 });                     // Noncompliant

        await dbConn.ColumnDistinctAsync<Entity>(query);                                                      // Compliant
        await dbConn.ColumnDistinctAsync<Entity>(query + param);                                              // Noncompliant
        OrmLiteReadApiAsync.ColumnDistinctAsync<Entity>(dbConn, query + param, new { Age = 42 });             // Noncompliant

        await dbConn.LookupAsync<string, Entity>(query);                                                      // Compliant
        await dbConn.LookupAsync<string, Entity>(query + param);                                              // Noncompliant
        OrmLiteReadApiAsync.LookupAsync<string, Entity>(dbConn, query + param, new { Age = 42 });             // Noncompliant

        await dbConn.DictionaryAsync<string, Entity>(query);                                                  // Compliant
        await dbConn.DictionaryAsync<string, Entity>(query + param);                                          // Noncompliant
        OrmLiteReadApiAsync.DictionaryAsync<string, Entity>(dbConn, query + param, new { Age = 42 });         // Noncompliant

        await dbConn.ExistsAsync<Entity>(query);                                                              // Compliant
        await dbConn.ExistsAsync<Entity>(query + param);                                                      // Noncompliant
        OrmLiteReadApiAsync.ExistsAsync<Entity>(dbConn, query + param, new { Age = 42 });                     // Noncompliant

        await dbConn.SqlListAsync<Entity>(query);                                                             // Compliant
        await dbConn.SqlListAsync<Entity>(query + param);                                                     // Noncompliant
        OrmLiteReadApiAsync.SqlListAsync<Entity>(dbConn, query + param, new { Age = 42 });                    // Noncompliant

        await dbConn.SqlColumnAsync<Entity>(query);                                                           // Compliant
        await dbConn.SqlColumnAsync<Entity>(query + param);                                                   // Noncompliant
        OrmLiteReadApiAsync.SqlColumnAsync<Entity>(dbConn, query + param, new { Age = 42 });                  // Noncompliant

        await dbConn.SqlScalarAsync<Entity>(query);                                                           // Compliant
        await dbConn.SqlScalarAsync<Entity>(query + param);                                                   // Noncompliant
        OrmLiteReadApiAsync.SqlScalarAsync<Entity>(dbConn, query + param, new { Age = 42 });                  // Noncompliant

        await dbConn.ExecuteNonQueryAsync(query);                                                             // Compliant
        await dbConn.ExecuteNonQueryAsync(query + param);                                                     // Noncompliant
        OrmLiteReadApiAsync.ExecuteNonQueryAsync(dbConn, query + param, new { Age = 42 });                    // Noncompliant
    }
}

class Entity
{
    public int Age { get; set; }
}
