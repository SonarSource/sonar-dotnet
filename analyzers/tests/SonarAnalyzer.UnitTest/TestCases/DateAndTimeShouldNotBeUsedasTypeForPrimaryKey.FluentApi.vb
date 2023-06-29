Imports Microsoft.EntityFrameworkCore
Imports System

Class FluentApi
    Class PersonDbContext
        Inherits DbContext
        Public Property People As DbSet(Of Person)

        Protected Overrides Sub OnModelCreating(ByVal modelBuilder As ModelBuilder)
            modelBuilder.Entity(Of Person)().HasKey(Function(x) x.DateOfBirth)
        End Sub
    End Class

    Class Person
        Public Property DateOfBirth As Date       ' FN - keys created with the Fluent API are too complex to track
    End Class
End Class
