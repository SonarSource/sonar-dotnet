using Dapper;
using System;
using System.Data;

namespace Tests.Diagnostics
{
    class DapperTest
    {
        private IDbConnection con = null;
        public void NonCompliant_Concat_Query(string query, string param)
        {
            con.Query("Select Name From Person Where Id=@Id", new { Id = param}); // Compliant
            con.Query(query + param);                                             // Noncompliant
            con.Query(typeof(object), query + param);                             // Noncompliant
            SqlMapper.Query(con, query + param);                                  // Noncompliant
            SqlMapper.Query(con, typeof(object), query + param);                  // FN. The string argument is in the third position for this overload invoked in the unreduced form
            con.Query(query + param, new { Id = 1 });                             // Noncompliant
            con.Query<DapperTest>(query + param, new { Id = 1 });                 // Noncompliant
        }
    }
}
