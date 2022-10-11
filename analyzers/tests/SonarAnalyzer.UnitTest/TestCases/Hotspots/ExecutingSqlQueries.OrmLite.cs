using System;
using System.Data;
using System.Linq;
using ServiceStack.OrmLite;

namespace Tests.Diagnostics
{
    class Program
    {
        public void OrmLiteReadApiMethods(IDbConnection dbConn, string query, string param)
        {
            dbConn.Select<Entity>(query + param); // Noncompliant
        }
    }

    class Entity { }
}
