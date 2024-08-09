Imports System
Imports System.Linq
Imports Microsoft.EntityFrameworkCore

Namespace Tests.Diagnostics
    Class Program
        Private Const ConstQuery As String = ""

        Public Sub Foo(ByVal context As DbContext, ByVal query As String, ByVal x As Int32, ParamArray parameters As Object())
            context.Database.ExecuteSqlRaw($"") ' Noncompliant
            context.Database.ExecuteSqlRaw("") ' Compliant, constants are safe
            context.Database.ExecuteSqlRaw(ConstQuery) ' Compliant, constants are safe
            context.Database.ExecuteSqlRaw("" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlRaw("" + "") ' Compliant, constants are safe
            context.Database.ExecuteSqlRaw(query) ' Compliant, not concat or format
            context.Database.ExecuteSqlRaw("" & query) ' Noncompliant
            context.Database.ExecuteSqlRaw("" + query) ' Noncompliant
            context.Database.ExecuteSqlRaw($"", parameters) ' Noncompliant
            context.Database.ExecuteSqlRaw(query, parameters) ' Compliant, not concat or format
            context.Database.ExecuteSqlRaw("" & query, parameters) ' Noncompliant
            context.Database.ExecuteSqlRaw("" + query, parameters) ' Noncompliant

            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={query}", parameters(0)) ' Noncompliant
            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={query}") ' Noncompliant

            RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(context.Database, query) ' Compliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(context.Database, $"SELECT * FROM mytable WHERE mycol={query} AND col2={0}", x) ' Noncompliant

            context.Database.ExecuteSqlRawAsync($"") ' Noncompliant
            context.Database.ExecuteSqlRawAsync("") ' Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync(ConstQuery) ' Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync("" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync("" + "") ' Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync("" + "" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync("" & "" + "") ' Compliant, constants are safe
            context.Database.ExecuteSqlRawAsync(query) ' Compliant, not concat or format
            context.Database.ExecuteSqlRawAsync("" & query) ' Noncompliant
            context.Database.ExecuteSqlRawAsync("" + query) ' Noncompliant
            context.Database.ExecuteSqlRawAsync($"", parameters) ' Noncompliant
            context.Database.ExecuteSqlRawAsync(query, parameters) ' Compliant, not concat or format
            context.Database.ExecuteSqlRawAsync("" & query, parameters) ' Noncompliant
            context.Database.ExecuteSqlRawAsync("" + query, parameters) ' Noncompliant
            context.Database.ExecuteSqlRawAsync(query + "", parameters) ' Noncompliant
            context.Database.ExecuteSqlRawAsync(query & "", parameters) ' Noncompliant
            context.Database.ExecuteSqlRawAsync("" & query + "", parameters) ' Noncompliant
            context.Database.ExecuteSqlRawAsync("" + query & "", parameters) ' Noncompliant
            context.Database.ExecuteSqlRawAsync($"SELECT * FROM mytable WHERE mycol={query}") ' Noncompliant
            context.Database.ExecuteSqlRawAsync($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "" & query, parameters) ' Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(context.Database, "" + query, parameters) ' Noncompliant

            context.Set(Of User)().FromSqlRaw($"") ' Noncompliant
            context.Set(Of User)().FromSqlRaw("") ' Compliant, constants are safe
            context.Set(Of User)().FromSqlRaw(ConstQuery) ' Compliant, constants are safe
            context.Set(Of User)().FromSqlRaw(query) ' Compliant, not concat or format
            context.Set(Of User)().FromSqlRaw("" & "") ' Compliant, constants are safe
            context.Set(Of User)().FromSqlRaw("" + "") ' Compliant, constants are safe
            context.Set(Of User)().FromSqlRaw($"", parameters) ' Noncompliant
            context.Set(Of User)().FromSqlRaw("", parameters) ' Compliant, the parameters are sanitized
            context.Set(Of User)().FromSqlRaw(query, parameters) ' Compliant, not concat or format
            context.Set(Of User)().FromSqlRaw("" & query, parameters) ' Noncompliant
            context.Set(Of User)().FromSqlRaw("" + query, parameters) ' Noncompliant
            context.Set(Of User)().FromSqlRaw($"SELECT * FROM mytable WHERE mycol={query}") ' Noncompliant
            context.Set(Of User)().FromSqlRaw($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            RelationalQueryableExtensions.FromSqlRaw(context.Set(Of User)(), "" & query, parameters) ' Noncompliant
            RelationalQueryableExtensions.FromSqlRaw(context.Set(Of User)(), "" + query, parameters) ' Noncompliant

        End Sub

        Public Sub ConcatAndFormat(ByVal context As DbContext, ByVal query As String, ParamArray parameters As Object())
            Dim formatted = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters) ' Secondary [1,2,3]
            Dim concatenated = String.Concat(query, parameters)                                    ' Secondary [4,5,6]
            Dim interpolated = $"SELECT * FROM mytable WHERE mycol={query}"                        ' Secondary [7,8,9]

            context.Database.ExecuteSqlRaw(String.Concat(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlRaw(String.Format(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlRaw(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            context.Database.ExecuteSqlRaw($"SELECT * FROM mytable WHERE mycol={parameters(0)}") ' Noncompliant
            context.Database.ExecuteSqlRaw(formatted)    ' Noncompliant [1]
            context.Database.ExecuteSqlRaw(concatenated) ' Noncompliant [4]
            context.Database.ExecuteSqlRaw(interpolated) ' Noncompliant [7]

            context.Database.ExecuteSqlRawAsync(String.Concat(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlRawAsync(String.Format(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlRawAsync(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            context.Database.ExecuteSqlRawAsync(formatted)    ' Noncompliant [2]
            context.Database.ExecuteSqlRawAsync(concatenated) ' Noncompliant [5]
            context.Database.ExecuteSqlRawAsync(interpolated) ' Noncompliant [8]

            context.Set(Of User)().FromSqlRaw(String.Concat(query, parameters)) ' Noncompliant
            context.Set(Of User)().FromSqlRaw(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            context.Set(Of User)().FromSqlRaw(formatted)    ' Noncompliant [3]
            context.Set(Of User)().FromSqlRaw(concatenated) ' Noncompliant [6]
            context.Set(Of User)().FromSqlRaw(interpolated) ' Noncompliant [9]
        End Sub

    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/9602
    Class Repro_9602
        Public Sub ConstantQuery(context As DbContext, onlyEnabled As Boolean)
            Dim query As String = "SELECT id FROM users"
            If onlyEnabled Then
                query += " WHERE enabled = 1"
            End If
            Dim query2 As String = $"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}" ' Secondary [c1, c2, c3]

            context.Database.ExecuteSqlRaw(query) ' Compliant
            context.Database.ExecuteSqlRaw(query2) ' Noncompliant [c1] - FP
            context.Database.ExecuteSqlRaw($"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}") ' Noncompliant - FP

            context.Database.ExecuteSqlRawAsync(query) ' Compliant
            context.Database.ExecuteSqlRawAsync(query2) ' Noncompliant [c2] - FP
            context.Database.ExecuteSqlRawAsync($"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}") ' Noncompliant - FP

            context.Set(Of User)().FromSqlRaw(query) ' Compliant
            context.Set(Of User)().FromSqlRaw(query2) ' Noncompliant [c3] - FP
            context.Set(Of User)().FromSqlRaw($"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}") ' Noncompliant - FP
        End Sub
    End Class

    Class User
        Private Property Id As String
        Private Property Name As String
    End Class
End Namespace
