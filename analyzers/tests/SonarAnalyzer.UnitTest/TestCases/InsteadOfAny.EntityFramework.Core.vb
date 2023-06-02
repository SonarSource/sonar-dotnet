Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.EntityFrameworkCore

Public Class EntityFrameworkTestcases
    Public Class MyEntity2
        Public Property Id As Integer
    End Class

    Public Class MyDbContext
        Inherits DbContext

        Public Property MyEntities As DbSet(Of MyEntity2)

        Public Sub GetEntities(ByVal dbContext As MyDbContext, ByVal ids As List(Of Integer))
            Dim __ As IQueryable(Of MyEntity2) = dbContext.MyEntities.Where(Function(e) ids.Any(Function(i) e.Id = i)) ' Compliant
            __ = dbContext.MyEntities.Where(Function(e) ids.Any(Function(i) e.Equals(i))) ' Compliant
            __ = dbContext.MyEntities.Where(Function(e) ids.Any(Function(i) e.Id > i)) ' Compliant
            __ = dbContext.MyEntities.Where(Function(e) ids.Any(Function(i) TypeOf e.Id Is i)) ' Error [BC30002]
        End Sub
    End Class
End Class
