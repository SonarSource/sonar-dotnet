Imports Dapper
Imports System.Data.SqlClient

' https://sonarsource.atlassian.net/browse/NET-1565
Public Class RepeatedParameterNamesInDatabase
    Public Sub ExecuteSqlCommandsForUsers(connection As SqlConnection)
        Dim query = "SELECT * FROM Users WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "John Doe")                          ' Compliant
        Dim result = connection.Query(Of User)(query, param)
    End Sub

    Public Sub ExecuteSqlCommandsForCompanies(connection As SqlConnection)
        Dim query = "SELECT * FROM Companies WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "Constosco")                         ' Compliant
        Dim result = connection.Query(Of Company)(query, param)
    End Sub

    Public Sub ExecuteSqlCommandsForProducts(connection As SqlConnection)
        Dim query = "SELECT * FROM Companies WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "CleanBot 9000")                     ' Compliant
        Dim result = connection.Query(Of Product)(query, param)
    End Sub

    Public Sub ExecuteSqlCommandsForCountries(connection As SqlConnection)
        Dim query = "SELECT * FROM Countries WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "Norway")                            ' Compliant
        Dim result = connection.Query(Of Country)(query, param)
    End Sub

    Public Class Product
    End Class
    Public Class Country
    End Class
    Public Class Company
    End Class
    Public Class User
    End Class
End Class
