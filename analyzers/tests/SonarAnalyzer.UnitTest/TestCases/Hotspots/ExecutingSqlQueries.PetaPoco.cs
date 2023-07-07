using System;
using System.Linq;
using System.Threading;
using PetaPoco;

class Program
{
    public void IExecuteMethods(IExecute execute, string query, string param)
    {
        execute.Execute(query);                                                                            // Compliant
        execute.Execute(query + param);                                                                    // Noncompliant

        execute.ExecuteScalar<Entity>(query);                                                              // Compliant
        execute.ExecuteScalar<Entity>(query + param);                                                      // Noncompliant
    }

    public void IDatabaseMethods(IDatabase database, string query, int param, int otherParam)
    {
        database.Execute(query);                                                                           // Compliant
        database.Execute(query + param);                                                                   // Noncompliant

        database.ExecuteScalar<Entity>(query);                                                             // Compliant
        database.ExecuteScalar<Entity>(query + param);                                                     // Noncompliant

        database.Query<Entity>(query);                                                                     // Compliant
        database.Query<Entity>(query + param);                                                             // Noncompliant
        database.Query<Entity>(query, param + otherParam);                                                 // Compliant
        database.Query<Entity>(new[] { typeof(Entity) }, null, query + param);                             // Noncompliant
        database.Query<Entity, Entity>(query + param);                                                     // Noncompliant
        database.Query<Entity, Entity, Entity>(query + param);                                             // Noncompliant
        database.Query<Entity, Entity, Entity, Entity>(query + param);                                     // Noncompliant
        database.Query<Entity, Entity, Entity, Entity, Entity>(query + param);                             // Noncompliant

        database.Fetch<Entity>(query);                                                                     // Compliant
        database.Fetch<Entity>(query + param);                                                             // Noncompliant
        database.Fetch<Entity>(2, 10, query + param);                                                      // Noncompliant
        database.Fetch<Entity>(param + otherParam, 10, query);                                             // Compliant
        database.Page<Entity>(0, 0, query + param, new object[0], query + param, new object[0]);
        //                          ^^^^^^^^^^^^^
        //                                                        ^^^^^^^^^^^^^                               @-1

        // All IQuery members
        database.Exists<Entity>(query + param);                                                            // Noncompliant
        database.Fetch<Entity, Entity>(query + param);                                                     // Noncompliant
        database.First<Entity>(query + param);                                                             // Noncompliant
        database.FirstOrDefault<Entity>(query + param);                                                    // Noncompliant
        database.Page<Entity>(0, 0, query + param);                                                        // Noncompliant (Parameter "sql")
        database.Page<Entity>(0, 0, query + param, Array.Empty<object>(), "", Array.Empty<object>());      // Noncompliant (Parameter "sqlCount")
        database.Page<Entity>(0, 0, "", Array.Empty<object>(), query + param, Array.Empty<object>());      // Noncompliant (Parameter "sqlPage")
        database.Query<Entity, Entity>(query + param);                                                     // Noncompliant
        database.QueryMultiple(query + param);                                                             // Noncompliant
        database.SkipTake<Entity>(0, 0, query + param);                                                    // Noncompliant
        database.Single<Entity>(query + param);                                                            // Noncompliant
        database.SingleOrDefault<Entity>(query + param);                                                   // Noncompliant

        // All IQueryAsync members
        database.ExistsAsync<Entity>(query + param);                                                       // Noncompliant
        database.FetchAsync<Entity>(query + param);                                                        // Noncompliant
        database.FirstAsync<Entity>(query + param);                                                        // Noncompliant
        database.FirstOrDefaultAsync<Entity>(query + param);                                               // Noncompliant
        database.PageAsync<Entity>(0, 0, query + param);                                                   // Noncompliant (Parameter "sql")
        database.PageAsync<Entity>(0, 0, query + param, Array.Empty<object>(), "", Array.Empty<object>()); // Noncompliant (Parameter "sqlCount")
        database.PageAsync<Entity>(0, 0, "", Array.Empty<object>(), query + param, Array.Empty<object>()); // Noncompliant (Parameter "sqlPage")
        database.QueryAsync<Entity>(query + param);                                                        // Noncompliant
        database.SkipTakeAsync<Entity>(0, 0, query + param);                                               // Noncompliant
        database.SingleAsync<Entity>(query + param);                                                       // Noncompliant
        database.SingleOrDefaultAsync<Entity>(query + param);                                              // Noncompliant

        // All IExecute and IExecuteAsync
        database.Execute(query + param);                                                                   // Noncompliant
        database.ExecuteAsync(CancellationToken.None, query + param);                                      // Noncompliant
        database.ExecuteScalar<Entity>(query + param);                                                     // Noncompliant
        database.ExecuteScalarAsync<Entity>(CancellationToken.None, query + param);                        // Noncompliant

        // All IAlterPoco and IAlterPocoAsync
        database.Update<Entity>(query + param);                                                            // Noncompliant
        database.Delete<Entity>(query + param);                                                            // Noncompliant
        database.UpdateAsync<Entity>(query + param);                                                       // Noncompliant
        database.UpdateAsync<Entity>(CancellationToken.None, query + param);                               // Noncompliant
        database.DeleteAsync<Entity>(query + param);                                                       // Noncompliant
        database.DeleteAsync<Entity>(CancellationToken.None, query + param);                               // Noncompliant
    }

    public void SqlType(string query, int param)
    {
        var s1 = new Sql(query + param);                                                                   // Noncompliant
        Sql s2 = new(query + param);                                                                       // Noncompliant
        Sql.Builder.Where(query + param);                                                                  // Noncompliant
    }
}

class Entity
{
    public int Id { get; set; }
}
