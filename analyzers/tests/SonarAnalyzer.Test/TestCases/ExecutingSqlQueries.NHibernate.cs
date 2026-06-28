using System;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg.Loquacious;
using NHibernate.Engine;
using NHibernate.Engine.Query;
using NHibernate.Engine.Query.Sql;
using NHibernate.Impl;

class Program
{
    public async Task ISessionMethods(ISession session, string query, string param)
    {
        session.CreateFilter(null, query);                                                              // Compliant
        session.CreateFilter(null, query + param);                                                      // Noncompliant

        await session.CreateFilterAsync(null, query);                                                   // Compliant
        await session.CreateFilterAsync(null, query + param);                                           // Noncompliant

        session.CreateQuery(query);                                                                     // Compliant
        session.CreateQuery(query + param);                                                             // Noncompliant

        session.CreateSQLQuery(query);                                                                  // Compliant
        session.CreateSQLQuery(query + param);                                                          // Noncompliant

        session.Delete(query);                                                                          // Compliant
        session.Delete(query + param);                                                                  // Noncompliant

        await session.DeleteAsync(query);                                                               // Compliant
        await session.DeleteAsync(query + param);                                                       // Noncompliant

        session.GetNamedQuery(query);                                                                   // Compliant
        session.GetNamedQuery(query + param);                                                           // Noncompliant
    }

    public async Task SessionImplMethods(SessionImpl session, string query, string param)
    {
        session.CreateFilter(null, query);                                                              // Compliant
        session.CreateFilter(null, query + param);                                                      // Noncompliant

        await session.CreateFilterAsync(null, query);                                                   // Compliant
        await session.CreateFilterAsync(null, query + param);                                           // Noncompliant

        session.CreateQuery(query);                                                                     // Compliant
        session.CreateQuery(query + param);                                                             // Noncompliant

        session.CreateSQLQuery(query);                                                                  // Compliant
        session.CreateSQLQuery(query + param);                                                          // Noncompliant

        session.Delete(query);                                                                          // Compliant
        session.Delete(query + param);                                                                  // Noncompliant

        await session.DeleteAsync(query);                                                               // Compliant
        await session.DeleteAsync(query + param);                                                       // Noncompliant

        session.GetNamedQuery(query);                                                                   // Compliant
        session.GetNamedQuery(query + param);                                                           // Noncompliant

        session.GetNamedSQLQuery(query);                                                                // Compliant
        session.GetNamedSQLQuery(query + param);                                                        // Noncompliant
    }

    public async Task AbstractSessionImplMethods(AbstractSessionImpl session, string query, string param)
    {
        session.CreateQuery(query);                                                                     // Compliant
        session.CreateQuery(query + param);                                                             // Noncompliant

        session.CreateSQLQuery(query);                                                                  // Compliant
        session.CreateSQLQuery(query + param);                                                          // Noncompliant

        session.GetNamedQuery(query);                                                                   // Compliant
        session.GetNamedQuery(query + param);                                                           // Noncompliant

        session.GetNamedSQLQuery(query);                                                                // Compliant
        session.GetNamedSQLQuery(query + param);                                                        // Noncompliant
    }

    public void NamedQueryDefinitionBuilder_Query(NamedQueryDefinitionBuilder builder, string query, string param)
    {
        builder.Query = query;                                                                          // Compliant
        builder.Query = query + param;                                                                  // Noncompliant
        builder.Query = $"SELECT * FROM table WHERE id = '{param}'";                                    // Noncompliant
        builder.Query = string.Format("SELECT * FROM table WHERE id = '{0}'", param);                   // Noncompliant
    }

    public void NamedQueryDefinition_Constructor(string query, string param)
    {
        new NamedQueryDefinition(query, false, null, 0, 0, FlushMode.Auto, CacheMode.Normal, false, null, null);              // Compliant
        new NamedQueryDefinition(query + param, false, null, 0, 0, FlushMode.Auto, CacheMode.Normal, false, null, null);      // Noncompliant
        new NamedQueryDefinition($"SELECT * FROM table WHERE id = '{param}'", false, null, 0, 0, FlushMode.Auto, CacheMode.Normal, false, null, null);  // Noncompliant
    }

    public void NamedSQLQueryDefinition_Constructor(string query, string param)
    {
        new NamedSQLQueryDefinition(query, (INativeSQLQueryReturn[])null, null, false, null, 0, 0, FlushMode.Auto, CacheMode.Normal, false, null, null, false);              // Compliant
        new NamedSQLQueryDefinition(query + param, (INativeSQLQueryReturn[])null, null, false, null, 0, 0, FlushMode.Auto, CacheMode.Normal, false, null, null, false);      // Noncompliant
        new NamedSQLQueryDefinition($"SELECT * FROM table WHERE id = '{param}'", (INativeSQLQueryReturn[])null, null, false, null, 0, 0, FlushMode.Auto, CacheMode.Normal, false, null, null, false);  // Noncompliant
    }

    public void QueryImpl_Constructor(ISessionImplementor sessionImplementor, ParameterMetadata parameterMetadata, string query, string param)
    {
        new QueryImpl(query, FlushMode.Auto, sessionImplementor, parameterMetadata);                    // Compliant
        new QueryImpl(query + param, FlushMode.Auto, sessionImplementor, parameterMetadata);            // Noncompliant
        new QueryImpl($"SELECT * FROM table WHERE id = '{param}'", FlushMode.Auto, sessionImplementor, parameterMetadata);  // Noncompliant
        new QueryImpl(string.Format("SELECT * FROM table WHERE id = '{0}'", param), FlushMode.Auto, sessionImplementor, parameterMetadata);  // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9602
class Repro_9602
{
    public async Task ConstantQuery(ISession session, bool onlyEnabled)
    {
        string query = "SELECT id FROM users";
        if(onlyEnabled)
            query += " WHERE enabled = 1";
        string query2 = $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"; // Secondary [s1, s2, s3, s4, s5, s6, s7, s8]

        session.CreateFilter(null, query);                                                              // Compliant
        session.CreateFilter(null, query2);                                                             // Noncompliant [s1] - FP
        session.CreateFilter(null, $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Noncompliant - FP

        session.CreateFilter(null, query);                                                              // Compliant
        session.CreateFilter(null, query2);                                                             // Noncompliant [s2] - FP
        session.CreateFilter(null, $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Noncompliant - FP

        await session.CreateFilterAsync(null, query);                                                   // Compliant
        await session.CreateFilterAsync(null, query2);                                                  // Noncompliant [s3] - FP
        await session.CreateFilterAsync(null, $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}");  // Noncompliant - FP

        session.CreateQuery(query);                                                                     // Compliant
        session.CreateQuery(query2);                                                                    // Noncompliant [s4] - FP
        session.CreateQuery($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}");        // Noncompliant - FP

        session.CreateSQLQuery(query);                                                                  // Compliant
        session.CreateSQLQuery(query2);                                                                 // Noncompliant [s5] - FP
        session.CreateSQLQuery($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}");     // Noncompliant - FP

        session.Delete(query);                                                                          // Compliant
        session.Delete(query2);                                                                         // Noncompliant [s6] - FP
        session.Delete($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}");             // Noncompliant - FP

        await session.DeleteAsync(query);                                                               // Compliant
        await session.DeleteAsync(query2);                                                              // Noncompliant [s7] - FP
        await session.DeleteAsync($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}");  // Noncompliant - FP

        session.GetNamedQuery(query);                                                                   // Compliant
        session.GetNamedQuery(query2);                                                                  // Noncompliant [s8] - FP
        session.GetNamedQuery($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}");      // Noncompliant - FP
    }
}
