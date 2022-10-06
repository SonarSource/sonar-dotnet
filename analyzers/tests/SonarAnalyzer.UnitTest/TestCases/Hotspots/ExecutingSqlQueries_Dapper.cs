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

        public void NonCompliant_Concat_QueryFirst(string query, string param)
        {
            con.QueryFirst("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            con.QueryFirst(query + param);                                             // Noncompliant
            con.QueryFirst(typeof(object), query + param);                             // Noncompliant
            SqlMapper.QueryFirst(con, query + param);                                  // Noncompliant
            SqlMapper.QueryFirst(con, typeof(object), query + param);                  // FN. The string argument is in the third position for this overload invoked in the unreduced form
            con.QueryFirst("", query + param);                                         // Compliant. The tracked strings are passed to the "param" object parameter
            con.QueryFirst(query + param, new { Id = 1 });                             // Noncompliant
            con.QueryFirst<DapperTest>(query + param, new { Id = 1 });                 // Noncompliant
        }

        public async Task NonCompliant_Concat_QueryFirstAsync(string query, string param)
        {
            await con.QueryFirstAsync("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            await con.QueryFirstAsync(query + param);                                              // Noncompliant
            await con.QueryFirstAsync(typeof(object), query + param);                              // Noncompliant
            await SqlMapper.QueryFirstAsync(con, query + param);                                   // Noncompliant
            await SqlMapper.QueryFirstAsync(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            await con.QueryFirstAsync("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            await con.QueryFirstAsync(query + param, new { Id = 1 });                              // Noncompliant
            await con.QueryFirstAsync<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }

        public void NonCompliant_Concat_QueryFirstOrDefault(string query, string param)
        {
            con.QueryFirstOrDefault("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            con.QueryFirstOrDefault(query + param);                                             // Noncompliant
            con.QueryFirstOrDefault(typeof(object), query + param);                             // Noncompliant
            SqlMapper.QueryFirstOrDefault(con, query + param);                                  // Noncompliant
            SqlMapper.QueryFirstOrDefault(con, typeof(object), query + param);                  // FN. The string argument is in the third position for this overload invoked in the unreduced form
            con.QueryFirstOrDefault("", query + param);                                         // Compliant. The tracked strings are passed to the "param" object parameter
            con.QueryFirstOrDefault(query + param, new { Id = 1 });                             // Noncompliant
            con.QueryFirstOrDefault<DapperTest>(query + param, new { Id = 1 });                 // Noncompliant
        }

        public async Task NonCompliant_Concat_QueryFirstOrDefaultAsync(string query, string param)
        {
            await con.QueryFirstOrDefaultAsync("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            await con.QueryFirstOrDefaultAsync(query + param);                                              // Noncompliant
            await con.QueryFirstOrDefaultAsync(typeof(object), query + param);                              // Noncompliant
            await SqlMapper.QueryFirstOrDefaultAsync(con, query + param);                                   // Noncompliant
            await SqlMapper.QueryFirstOrDefaultAsync(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            await con.QueryFirstOrDefaultAsync("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            await con.QueryFirstOrDefaultAsync(query + param, new { Id = 1 });                              // Noncompliant
            await con.QueryFirstOrDefaultAsync<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }
    }
}
