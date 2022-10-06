using Dapper;
using System;
using System.Data;

namespace Tests.Diagnostics
{
    class DapperTest
    {
        private IDbConnection con = null;
        public void NonCompliant_Concat_SqlCommands(string query, string param)
        {
            con.Query(query + param); // Noncompliant
        }
    }
}
