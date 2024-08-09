using System;
using System.Linq;
using System.Threading.Tasks;
using NHibernate;
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
