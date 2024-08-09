using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    class DapperTest
    {
        private IDbConnection con = null;

        public void SqlMapper_Query(string query, string param)
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

        public async Task SqlMapper_QueryAsync(string query, string param)
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

        public void SqlMapper_QueryFirst(string query, string param)
        {
            con.QueryFirst("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            con.QueryFirst(query + param);                                              // Noncompliant
            con.QueryFirst(typeof(object), query + param);                              // Noncompliant
            SqlMapper.QueryFirst(con, query + param);                                   // Noncompliant
            SqlMapper.QueryFirst(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            con.QueryFirst("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            con.QueryFirst(query + param, new { Id = 1 });                              // Noncompliant
            con.QueryFirst<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }

        public async Task SqlMapper_QueryFirstAsync(string query, string param)
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

        public void SqlMapper_QueryFirstOrDefault(string query, string param)
        {
            con.QueryFirstOrDefault("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            con.QueryFirstOrDefault(query + param);                                              // Noncompliant
            con.QueryFirstOrDefault(typeof(object), query + param);                              // Noncompliant
            SqlMapper.QueryFirstOrDefault(con, query + param);                                   // Noncompliant
            SqlMapper.QueryFirstOrDefault(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            con.QueryFirstOrDefault("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            con.QueryFirstOrDefault(query + param, new { Id = 1 });                              // Noncompliant
            con.QueryFirstOrDefault<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }

        public async Task SqlMapper_QueryFirstOrDefaultAsync(string query, string param)
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

        public void SqlMapper_QuerySingle(string query, string param)
        {
            con.QuerySingle("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            con.QuerySingle(query + param);                                              // Noncompliant
            con.QuerySingle(typeof(object), query + param);                              // Noncompliant
            SqlMapper.QuerySingle(con, query + param);                                   // Noncompliant
            SqlMapper.QuerySingle(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            con.QuerySingle("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            con.QuerySingle(query + param, new { Id = 1 });                              // Noncompliant
            con.QuerySingle<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }

        public async Task SqlMapper_QuerySingleAsync(string query, string param)
        {
            await con.QuerySingleAsync("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            await con.QuerySingleAsync(query + param);                                              // Noncompliant
            await con.QuerySingleAsync(typeof(object), query + param);                              // Noncompliant
            await SqlMapper.QuerySingleAsync(con, query + param);                                   // Noncompliant
            await SqlMapper.QuerySingleAsync(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            await con.QuerySingleAsync("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            await con.QuerySingleAsync(query + param, new { Id = 1 });                              // Noncompliant
            await con.QuerySingleAsync<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }

        public void SqlMapper_QuerySingleOrDefault(string query, string param)
        {
            con.QuerySingleOrDefault("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            con.QuerySingleOrDefault(query + param);                                              // Noncompliant
            con.QuerySingleOrDefault(typeof(object), query + param);                              // Noncompliant
            SqlMapper.QuerySingleOrDefault(con, query + param);                                   // Noncompliant
            SqlMapper.QuerySingleOrDefault(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            con.QuerySingleOrDefault("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            con.QuerySingleOrDefault(query + param, new { Id = 1 });                              // Noncompliant
            con.QuerySingleOrDefault<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }

        public async Task SqlMapper_QuerySingleOrDefaultAsync(string query, string param)
        {
            await con.QuerySingleOrDefaultAsync("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            await con.QuerySingleOrDefaultAsync(query + param);                                              // Noncompliant
            await con.QuerySingleOrDefaultAsync(typeof(object), query + param);                              // Noncompliant
            await SqlMapper.QuerySingleOrDefaultAsync(con, query + param);                                   // Noncompliant
            await SqlMapper.QuerySingleOrDefaultAsync(con, typeof(object), query + param);                   // FN. The string argument is in the third position for this overload invoked in the unreduced form
            await con.QuerySingleOrDefaultAsync("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            await con.QuerySingleOrDefaultAsync(query + param, new { Id = 1 });                              // Noncompliant
            await con.QuerySingleOrDefaultAsync<DapperTest>(query + param, new { Id = 1 });                  // Noncompliant
        }

        public void SqlMapper_QueryMultiple(string query, string param)
        {
            con.QueryMultiple("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            con.QueryMultiple(query + param);                                              // Noncompliant
            SqlMapper.QueryMultiple(con, query + param);                                   // Noncompliant
            con.QueryMultiple("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            con.QueryMultiple(query + param, new { Id = 1 });                              // Noncompliant
        }

        public async Task SqlMapper_QueryMultipleAsync(string query, string param)
        {
            await con.QueryMultipleAsync("Select Name From Person Where Id=@Id", new { Id = param }); // Compliant
            await con.QueryMultipleAsync(query + param);                                              // Noncompliant
            await SqlMapper.QueryMultipleAsync(con, query + param);                                   // Noncompliant
            await con.QueryMultipleAsync("", query + param);                                          // Compliant. The tracked strings are passed to the "param" object parameter
            await con.QueryMultipleAsync(query + param, new { Id = 1 });                              // Noncompliant
        }

        public void SqlMapper_Execute(string query, string param)
        {
            con.Execute("Insert Into Person Values (Id=@Id)", new { Id = param }); // Compliant
            con.Execute(query + param);                                            // Noncompliant
            con.Execute("", query + param);                                        // Compliant. The tracked strings are passed to the "param" object parameter
            con.Execute(query + param, new { Id = 1 });                            // Noncompliant
            SqlMapper.Execute(con, query + param);                                 // Noncompliant
        }

        public async Task SqlMapper_ExecuteAsync(string query, string param)
        {
            await con.ExecuteAsync("Insert Into Person Values (Id=@Id)", new { Id = param }); // Compliant
            await con.ExecuteAsync(query + param);                                            // Noncompliant
            await con.ExecuteAsync("", query + param);                                        // Compliant. The tracked strings are passed to the "param" object parameter
            await con.ExecuteAsync(query + param, new { Id = 1 });                            // Noncompliant
            await SqlMapper.ExecuteAsync(con, query + param);                                 // Noncompliant
        }

        public void SqlMapper_ExecuteReader(string query, string param)
        {
            con.ExecuteReader("Insert Into Person Values (Id=@Id)", new { Id = param }); // Compliant
            con.ExecuteReader(query + param);                                            // Noncompliant
            con.ExecuteReader("", query + param);                                        // Compliant. The tracked strings are passed to the "param" object parameter
            con.ExecuteReader(query + param, new { Id = 1 });                            // Noncompliant
            SqlMapper.ExecuteReader(con, query + param);                                 // Noncompliant
        }

        public async Task SqlMapper_ExecuteReaderAsync(string query, string param)
        {
            await con.ExecuteReaderAsync("Insert Into Person Values (Id=@Id)", new { Id = param }); // Compliant
            await con.ExecuteReaderAsync(query + param);                                            // Noncompliant
            await con.ExecuteReaderAsync("", query + param);                                        // Compliant. The tracked strings are passed to the "param" object parameter
            await con.ExecuteReaderAsync(query + param, new { Id = 1 });                            // Noncompliant
            await SqlMapper.ExecuteReaderAsync(con, query + param);                                 // Noncompliant
        }

        public void SqlMapper_ExecuteScalar(string query, string param)
        {
            con.ExecuteScalar("Insert Into Person Values (Id=@Id)", new { Id = param }); // Compliant
            con.ExecuteScalar(query + param);                                            // Noncompliant
            con.ExecuteScalar("", query + param);                                        // Compliant. The tracked strings are passed to the "param" object parameter
            con.ExecuteScalar(query + param, new { Id = 1 });                            // Noncompliant
            con.ExecuteScalar<DapperTest>(query + param);                                // Noncompliant
            SqlMapper.ExecuteScalar(con, query + param);                                 // Noncompliant
        }

        public async Task SqlMapper_ExecuteScalarAsync(string query, string param)
        {
            await con.ExecuteScalarAsync("Insert Into Person Values (Id=@Id)", new { Id = param }); // Compliant
            await con.ExecuteScalarAsync(query + param);                                            // Noncompliant
            await con.ExecuteScalarAsync("", query + param);                                        // Compliant. The tracked strings are passed to the "param" object parameter
            await con.ExecuteScalarAsync(query + param, new { Id = 1 });                            // Noncompliant
            await con.ExecuteScalarAsync<DapperTest>(query + param);                                // Noncompliant
            await SqlMapper.ExecuteScalarAsync(con, query + param);                                 // Noncompliant
        }

        public void CommandDefinition_Constructor(string query, string param)
        {
            new CommandDefinition("Insert Into Person Values (Id=@Id)", new { Id = param }); // Compliant
            new CommandDefinition(query + param);                                            // Noncompliant
            new CommandDefinition(query + param, new { Id = 1 });                            // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9602
    class Repro_9602
    {
        public void ConstantQuery(IDbConnection dbConnection, bool onlyEnabled)
        {
            string query = "SELECT id FROM users";
            if(onlyEnabled)
                query += " WHERE enabled = 1";
            string query2 = $"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"; // Secondary - FP

            dbConnection.Query<int>(query); // Compliant
            dbConnection.Query<int>(query2); // Noncompliant - FP
            dbConnection.Query<int>($"SELECT id FROM users {(onlyEnabled ? "WHERE enabled = 1" : "")}"); // Noncompliant - FP
        }
    }
}
