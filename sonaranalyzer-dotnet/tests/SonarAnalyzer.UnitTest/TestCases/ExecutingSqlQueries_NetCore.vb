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
            context.Database.ExecuteSqlCommand(query) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommand("" & query) ' Noncompliant
            context.Database.ExecuteSqlCommand($"", parameters) ' Noncompliant, interpolated string with argument transformed in RawQuery
            context.Database.ExecuteSqlCommand(query, parameters) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommand("" & query, parameters) ' Noncompliant

            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}", parameters(0)) ' Noncompliant, the FormattableString is evaluated and converted to RawSqlString
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={query}") ' Compliant, FormattableString is sanitized

            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, query) ' Compliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommand(context.Database, $"SELECT * FROM mytable WHERE mycol={query} AND col2={0}", x) ' Noncompliant

            context.Database.ExecuteSqlCommandAsync($"") ' Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync("") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(ConstQuery) ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(query) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommandAsync("" & query) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync($"", parameters) ' Noncompliant, interpolated string with argument transformed in RawQuery
            context.Database.ExecuteSqlCommandAsync(query, parameters) ' Compliant, not concat or format
            context.Database.ExecuteSqlCommandAsync("" & query, parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync($"SELECT * FROM mytable WHERE mycol={query}") ' Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            RelationalDatabaseFacadeExtensions.ExecuteSqlCommandAsync(context.Database, "" & query, parameters) ' Noncompliant

            context.Query(Of User)().FromSql($"") ' Compliant, FormattableString is sanitized
            context.Query(Of User)().FromSql("") ' Compliant, constants are safe
            context.Query(Of User)().FromSql(ConstQuery) ' Compliant, constants are safe
            context.Query(Of User)().FromSql(query) ' Compliant, not concat or format
            context.Query(Of User)().FromSql("" & "") ' Compliant, constants are safe
            context.Query(Of User)().FromSql($"", parameters) ' Noncompliant, interpolated string with argument transformed in RawQuery
            context.Query(Of User)().FromSql("", parameters) ' Compliant, the parameters are sanitized
            context.Query(Of User)().FromSql(query, parameters) ' Compliant, not concat or format
            context.Query(Of User)().FromSql("" & query, parameters) ' Noncompliant
            context.Query(Of User)().FromSql($"SELECT * FROM mytable WHERE mycol={query}") ' Compliant, FormattableString is sanitized
            context.Query(Of User)().FromSql($"SELECT * FROM mytable WHERE mycol={query}", x) ' Noncompliant
            RelationalQueryableExtensions.FromSql(context.Query(Of User)(), "" & query, parameters) ' Noncompliant

        End Sub

        Public Sub ConcatAndFormat(ByVal context As DbContext, ByVal query As String, ParamArray parameters As Object())
            context.Database.ExecuteSqlCommand(String.Concat(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommand(String.Format(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommand(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommand($"SELECT * FROM mytable WHERE mycol={parameters(0)}") ' Compliant, is sanitized
            Dim formatted = String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)
            context.Database.ExecuteSqlCommand(formatted) ' FN
            context.Database.ExecuteSqlCommandAsync(String.Concat(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync(String.Format(query, parameters)) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
            Dim concatenated = String.Concat(query, parameters)
            context.Database.ExecuteSqlCommandAsync(concatenated) ' FN
            context.Query(Of User)().FromSql(String.Concat(query, parameters)) ' Noncompliant
            context.Query(Of User)().FromSql(String.Format("INSERT INTO Users (name) VALUES (""{0}"")", parameters)) ' Noncompliant
        End Sub

    End Class

    Class User
        Private Property Id As String
        Private Property Name As String
    End Class
End Namespace
