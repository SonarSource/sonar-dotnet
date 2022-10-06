using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

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
            con.Query("", query + param);                                         // Compliant. The tracked strings are passed to the "param" object parameter
            con.Query(query + param, new { Id = 1 });                             // Noncompliant
            con.Query<DapperTest>(query + param, new { Id = 1 });                 // Noncompliant
        }

        public async Task NonCompliant_Concat_QueryAsync(string query, string param)
        {
            await con.QueryAsync("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            await con.QueryAsync(query + param);                                              // Noncompliant
            await con.QueryAsync(typeof(object), query + param);                              // Noncompliant
            await SqlMapper.QueryAsync(con, query + param);                                   // Noncompliant
            await SqlMapper.QueryAsync(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            await con.QueryAsync("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            await con.QueryAsync(query + param, new { Id = 1 });                              // Noncompliant
            await con.QueryAsync<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }
    }
}
