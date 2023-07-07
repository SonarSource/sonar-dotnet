Imports System
Imports System.Linq
Imports System.Threading
Imports PetaPoco

Class Program
    Public Sub IExecuteMethods(ByVal execute As IExecute, ByVal query As String, ByVal param As String)
        execute.Execute(query)                                                                                      ' Compliant
        execute.Execute(query & param)                                                                              ' Noncompliant
        execute.ExecuteScalar(Of Entity)(query)                                                                     ' Compliant
        execute.ExecuteScalar(Of Entity)(query & param)                                                             ' Noncompliant
    End Sub

    Public Sub IDatabaseMethods(ByVal database As IDatabase, ByVal query As String, ByVal param As Integer, ByVal otherParam As Integer)
        database.Execute(query)                                                                                     ' Compliant
        database.Execute(query & param)                                                                             ' Noncompliant
        database.ExecuteScalar(Of Entity)(query)                                                                    ' Compliant
        database.ExecuteScalar(Of Entity)(query & param)                                                            ' Noncompliant
        database.Query(Of Entity)(query)                                                                            ' Compliant
        database.Query(Of Entity)(query & param)                                                                    ' Noncompliant
        database.Query(Of Entity)(query, param + otherParam)                                                        ' Compliant
        database.Query(Of Entity)({GetType(Entity)}, Nothing, query & param)                                        ' Noncompliant
        database.Query(Of Entity, Entity)(query & param)                                                            ' Noncompliant
        database.Query(Of Entity, Entity, Entity)(query & param)                                                    ' Noncompliant
        database.Query(Of Entity, Entity, Entity, Entity)(query & param)                                            ' Noncompliant
        database.Query(Of Entity, Entity, Entity, Entity, Entity)(query & param)                                    ' Noncompliant

        database.Fetch(Of Entity)(query)                                                                            ' Compliant
        database.Fetch(Of Entity)(query & param)                                                                    ' Noncompliant
        database.Fetch(Of Entity)(query & param)                                                                    ' Noncompliant
        database.Fetch(Of Entity)(2, 10, query & param)                                                             ' Noncompliant
        database.Fetch(Of Entity)(param + otherParam, 10, query)                                                    ' Compliant
        database.Page(Of Entity)(0, 0, query & param, New Object(-1) {}, query & param, New Object(-1) {})
        '                              ^^^^^^^^^^^^^
        '                                                                ^^^^^^^^^^^^^                                @-1
        database.Exists(Of Entity)(query & param)                                                                   ' Noncompliant
        database.Fetch(Of Entity, Entity)(query & param)                                                            ' Noncompliant
        database.First(Of Entity)(query & param)                                                                    ' Noncompliant
        database.FirstOrDefault(Of Entity)(query & param)                                                           ' Noncompliant
        database.Page(Of Entity)(0, 0, query & param)                                                               ' Noncompliant
        database.Page(Of Entity)(0, 0, query & param, Array.Empty(Of Object)(), "", Array.Empty(Of Object)())       ' Noncompliant
        database.Page(Of Entity)(0, 0, "", Array.Empty(Of Object)(), query & param, Array.Empty(Of Object)())       ' Noncompliant
        database.Query(Of Entity, Entity)(query & param)                                                            ' Noncompliant
        database.QueryMultiple(query & param)                                                                       ' Noncompliant
        database.SkipTake(Of Entity)(0, 0, query & param)                                                           ' Noncompliant
        database.Single(Of Entity)(query & param)                                                                   ' Noncompliant
        database.SingleOrDefault(Of Entity)(query & param)                                                          ' Noncompliant
        database.ExistsAsync(Of Entity)(query & param)                                                              ' Noncompliant
        database.FetchAsync(Of Entity)(query & param)                                                               ' Noncompliant
        database.FirstAsync(Of Entity)(query & param)                                                               ' Noncompliant
        database.FirstOrDefaultAsync(Of Entity)(query & param)                                                      ' Noncompliant
        database.PageAsync(Of Entity)(0, 0, query & param)                                                          ' Noncompliant
        database.PageAsync(Of Entity)(0, 0, query & param, Array.Empty(Of Object)(), "", Array.Empty(Of Object)())  ' Noncompliant
        database.PageAsync(Of Entity)(0, 0, "", Array.Empty(Of Object)(), query & param, Array.Empty(Of Object)())  ' Noncompliant
        database.QueryAsync(Of Entity)(query & param)                                                               ' Noncompliant
        database.SkipTakeAsync(Of Entity)(0, 0, query & param)                                                      ' Noncompliant
        database.SingleAsync(Of Entity)(query & param)                                                              ' Noncompliant
        database.SingleOrDefaultAsync(Of Entity)(query & param)                                                     ' Noncompliant
        database.Execute(query & param)                                                                             ' Noncompliant
        database.ExecuteAsync(CancellationToken.None, query & param)                                                ' Noncompliant
        database.ExecuteScalar(Of Entity)(query & param)                                                            ' Noncompliant
        database.ExecuteScalarAsync(Of Entity)(CancellationToken.None, query & param)                               ' Noncompliant
        database.Update(Of Entity)(query & param)                                                                   ' Noncompliant
        database.Delete(Of Entity)(query & param)                                                                   ' Noncompliant
        database.UpdateAsync(Of Entity)(query & param)                                                              ' Noncompliant
        database.UpdateAsync(Of Entity)(CancellationToken.None, query & param)                                      ' Noncompliant
        database.DeleteAsync(Of Entity)(query & param)                                                              ' Noncompliant
        database.DeleteAsync(Of Entity)(CancellationToken.None, query & param)                                      ' Noncompliant
    End Sub

    Public Sub SqlType(ByVal query As String, ByVal param As Integer)
        Dim s1 = New Sql(query & param)                                                                             ' Noncompliant
        Sql.Builder.Where(query & param)                                                                            ' Noncompliant
    End Sub
End Class

Class Entity
    Public Property Id As Integer
End Class
