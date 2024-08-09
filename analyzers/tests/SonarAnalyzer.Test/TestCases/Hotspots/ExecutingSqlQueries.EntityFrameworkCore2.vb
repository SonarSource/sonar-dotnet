Imports System
Imports System.Linq
Imports Microsoft.EntityFrameworkCore

Namespace Tests.Diagnostics
    Class Program
        Private Const ConstQuery As String = ""

        Public Sub Foo(ByVal context As DbContext, ByVal query As String, ByVal x As Int32, ParamArray parameters As Object())
            context.Database.ExecuteSqlCommand($"") ' Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommand("") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommand(ConstQuery) ' Compliant, constants are safe
            context.Database.ExecuteSqlCommand("" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommand("" + "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommand(query) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommand("" & query) ' Noncompliant
            context.Database.ExecuteSqlCommand("" + query) ' Noncompliant
            context.Database.ExecuteSqlCommand($"", parameters) ' Noncompliant, interpolated string with argument transformed in RawQuery
            context.Database.ExecuteSqlCommand(query, parameters) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommand("" & query, parameters) ' Noncompliant
            context.Database.ExecuteSqlCommand("" + query, parameters) ' Noncompliant

            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}", parameters(0)) ' Noncompliant, the FormattableString is evaluated and converted to RawSqlString
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}") ' Compliant, FormattableString is sanitized

            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, query) ' Compliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, $"SELECT * FROM mytable WHERE mycol={query} AND col2={0}", x) ' Noncompliant

            context.Database.ExecuteSqlCommandAsync($"") ' Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync("") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(ConstQuery) ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" + "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" + "" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" & "" + "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(query) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommandAsync("" & query) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync("" + query) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync($"", parameters) ' Noncompliant, interpolated string with argument transformed in RawQuery
            context.Database.ExecuteSqlCommandAsync(query, parameters) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommandAsync("" & query, parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync("" + query, parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync(query + "", parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync(query & "", parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync("" & query + "", parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync("" + query & "", parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync($"SELECT * FROM mytable WHERE mycol={query}") ' Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommandAsync(context.Database, "" & query, parameters) ' Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommandAsync(context.Database, "" + query, parameters) ' Noncompliant

            context.Query(Of User)().FromSql($"") ' Compliant, FormattableString is sanitized
            context.Query(Of User)().FromSql("") ' Compliant, constants are safe
            context.Query(Of User)().FromSql(ConstQuery) ' Compliant, constants are safe
            context.Query(Of User)().FromSql(query) ' Compliant, not concat or format
            context.Query(Of User)().FromSql("" & "") ' Compliant, constants are safe
            context.Query(Of User)().FromSql("" + "") ' Compliant, constants are safe
            context.Query(Of User)().FromSql($"", parameters) ' Noncompliant, interpolated string with argument transformed in RawQuery
            context.Query(Of User)().FromSql("", parameters) ' Compliant, the parameters are sanitized
            context.Query(Of User)().FromSql(query, parameters) ' Compliant, not concat or format
            context.Query(Of User)().FromSql("" & query, parameters) ' Noncompliant
            context.Query(Of User)().FromSql("" + query, parameters) ' Noncompliant
            context.Query(Of User)().FromSql($"SELECT * FROM mytable WHERE mycol={query}") ' Compliant, FormattableString is sanitized
            context.Query(Of User)().FromSql($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            RelationalQueryableExtensions.FromSql(context.Query(Of User)(), "" & query, parameters) ' Noncompliant
            RelationalQueryableExtensions.FromSql(context.Query(Of User)(), "" + query, parameters) ' Noncompliant

        End Sub

        Public Sub ConcatAndFormat(ByVal context As DbContext, ByVal query As String, ParamArray parameters As Object())
            Dim formatted = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters) ' Secondary [1,2,3]
            Dim concatenated = String.Concat(query, parameters)                                    ' Secondary [4,5,6]
            Dim interpolated = $"SELECT * FROM mytable WHERE mycol={query}"                        ' Secondary [7,8,9]

            context.Database.ExecuteSqlCommand(String.Concat(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommand(String.Format(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={parameters(0)}") ' Compliant, is sanitized
            context.Database.ExecuteSqlCommand(formatted)    ' Noncompliant [1]
            context.Database.ExecuteSqlCommand(concatenated) ' Noncompliant [4]
            context.Database.ExecuteSqlCommand(interpolated) ' Noncompliant [7]

            context.Database.ExecuteSqlCommandAsync(String.Concat(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync(String.Format(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync(formatted)    ' Noncompliant [2]
            context.Database.ExecuteSqlCommandAsync(concatenated) ' Noncompliant [5]
            context.Database.ExecuteSqlCommandAsync(interpolated) ' Noncompliant [8]

            context.Query(Of User)().FromSql(String.Concat(query, parameters)) ' Noncompliant
            context.Query(Of User)().FromSql(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            context.Query(Of User)().FromSql(formatted)    ' Noncompliant [3]
            context.Query(Of User)().FromSql(concatenated) ' Noncompliant [6]
            context.Query(Of User)().FromSql(interpolated) ' Noncompliant [9]
        End Sub

    End Class

    ' https://github.com/SonarSource/sonar-dotnet/issues/9602
    Class Repro_9602
        Public Sub ConstantQuery(context As DbContext, onlyEnabled As Boolean)
            Dim query As String = "SELECT id FROM users"
            If onlyEnabled Then
                query += " WHERE enabled = 1"
            End If
            Dim query2 As String = $"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}" ' Secondary [c1,c2,c3]

            context.Database.ExecuteSqlCommand(query) ' Compliant
            context.Database.ExecuteSqlCommand(query2) ' Noncompliant [c1] - FP
            context.Database.ExecuteSqlCommand($"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}") ' Compliant

            context.Database.ExecuteSqlCommandAsync(query) ' Compliant
            context.Database.ExecuteSqlCommandAsync(query2) ' Noncompliant [c2] - FP
            context.Database.ExecuteSqlCommandAsync($"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}") ' Compliant

            context.Query(Of User)().FromSql(query) ' Compliant
            context.Query(Of User)().FromSql(query2) ' Noncompliant [c3] - FP
            context.Query(Of User)().FromSql($"SELECT id FROM users {(If(onlyEnabled, "WHERE enabled = 1", ""))}") ' Compliant
        End Sub
    End Class

    Class User
        Private Property Id As String
        Private Property Name As String
    End Class
End Namespace
