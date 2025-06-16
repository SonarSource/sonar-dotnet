Imports Dapper
Imports System.Data.SqlClient

' https://github.com/SonarSource/sonar-dotnet/issues/9569
Public Class RepeatedParameterNamesInDatabase
    Public Sub ExecuteSqlCommandsForUsers(connection As SqlConnection)
        Dim query = "SELECT * FROM Users WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "John Doe")                          ' Noncompliant - FP: Name refers to parameters in different SQL tables.
        Dim result = connection.Query(Of User)(query, param)    ' Renaming one does not necessitate renaming of parameters with the same name from other tables.
    End Sub

    Public Sub ExecuteSqlCommandsForCompanies(connection As SqlConnection)
        Dim query = "SELECT * FROM Companies WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "Constosco")                         ' Secondary - FP
        Dim result = connection.Query(Of Company)(query, param)
    End Sub

    Public Sub ExecuteSqlCommandsForProducts(connection As SqlConnection)
        Dim query = "SELECT * FROM Companies WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "CleanBot 9000")                     ' Secondary - FP
        Dim result = connection.Query(Of Product)(query, param)
    End Sub

    Public Sub ExecuteSqlCommandsForCountries(connection As SqlConnection)
        Dim query = "SELECT * FROM Countries WHERE Name = @name"
        Dim param = New DynamicParameters()
        param.Add("@name", "Norway")                            ' Secondary - FP
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
