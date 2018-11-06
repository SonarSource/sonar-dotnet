Imports System
Imports System.Linq
Imports Microsoft.EntityFrameworkCore

Namespace Tests.Diagnostics
    Class Program
        Private Const ConstQuery As String = ""

        Public Sub Foo(ByVal context As DbContext, ByVal query As String, ParamArray parameters As Object())
            context.Database.ExecuteSqlCommand($"") ' Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommand("") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommand(ConstQuery) ' Compliant, constants are safe
            context.Database.ExecuteSqlCommand("" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommand(query) ' Noncompliant
            context.Database.ExecuteSqlCommand("" & query) ' Noncompliant
            context.Database.ExecuteSqlCommand($"", parameters) ' Noncompliant, the FormattableString is evaluated before and is not sanitized
            context.Database.ExecuteSqlCommand(query, parameters) ' Noncompliant
            context.Database.ExecuteSqlCommand("" & query, parameters) ' Noncompliant

            context.Database.ExecuteSqlCommandAsync($"") ' Compliant, FormattableString is sanitized
            context.Database.ExecuteSqlCommandAsync("") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(ConstQuery) ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync("" & "") ' Compliant, constants are safe
            context.Database.ExecuteSqlCommandAsync(query) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync("" & query) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync($"", parameters) ' Noncompliant, the FormattableString is evaluated before and is not sanitized
            context.Database.ExecuteSqlCommandAsync(query, parameters) ' Noncompliant
            context.Database.ExecuteSqlCommandAsync("" & query, parameters) ' Noncompliant

            context.Query(Of User)().FromSql($"") ' Compliant, FormattableString is sanitized
            context.Query(Of User)().FromSql("") ' Compliant, constants are safe
            context.Query(Of User)().FromSql(ConstQuery) ' Compliant, constants are safe
            context.Query(Of User)().FromSql(query) ' Noncompliant
            context.Query(Of User)().FromSql("" & "") ' Compliant, constants are safe
            context.Query(Of User)().FromSql($"", parameters) ' Noncompliant, the FormattableString is evaluated before and is not sanitized
            context.Query(Of User)().FromSql("", parameters) ' Compliant, the parameters are sanitized
            context.Query(Of User)().FromSql(query, parameters) ' Noncompliant, even though the parameters are sanitized, query could be tainted
            context.Query(Of User)().FromSql("" & query, parameters) ' Noncompliant
        End Sub
    End Class

    Class User
        Private Property Id As String
        Private Property Name As String
    End Class
End Namespace
