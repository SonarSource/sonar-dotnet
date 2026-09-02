Imports Microsoft.EntityFrameworkCore
Imports Microsoft.EntityFrameworkCore.Metadata.Builders

' https://sonarsource.atlassian.net/browse/NET-3281
Namespace EntityFrameworkModelConfiguration
    Public Class Entity
    End Class

    Public Class Context
        Inherits DbContext

        Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
            Dim columnNames = {"ColumnName", "ColumnName", "ColumnName", "ColumnName"} ' Compliant - model configuration
            Dim configure = Sub()
                                Dim nestedColumnNames = {"NestedColumn", "NestedColumn", "NestedColumn", "NestedColumn"} ' Compliant - nested model configuration
                            End Sub
        End Sub

        Public Overloads Sub OnModelCreating()
            Dim values = {
                "NotAnOverride", ' Noncompliant {{Define a constant instead of using this literal 'NotAnOverride' 4 times.}}
                "NotAnOverride", ' Secondary
                "NotAnOverride", ' Secondary
                "NotAnOverride"  ' Secondary
            }
        End Sub
    End Class

    Public MustInherit Class BaseAppContext
        Inherits DbContext

        Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
        End Sub
    End Class

    Public Class DerivedAppContext
        Inherits BaseAppContext

        Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
            Dim columnNames = {"InheritedColumn", "InheritedColumn", "InheritedColumn", "InheritedColumn"} ' Compliant - model configuration
        End Sub
    End Class

    Public Class EntityConfiguration
        Implements IEntityTypeConfiguration(Of Entity)

        Public Overloads Sub Configure(builder As EntityTypeBuilder(Of Entity)) Implements IEntityTypeConfiguration(Of Entity).Configure
            Dim columnNames = {"ColumnName", "ColumnName", "ColumnName", "ColumnName"} ' Compliant - model configuration
        End Sub

        Public Overloads Sub Configure(value As String)
            Dim values = {
                "NotInterfaceImplementation", ' Noncompliant {{Define a constant instead of using this literal 'NotInterfaceImplementation' 4 times.}}
                "NotInterfaceImplementation", ' Secondary
                "NotInterfaceImplementation", ' Secondary
                "NotInterfaceImplementation"  ' Secondary
            }
        End Sub
    End Class

    Public Class RenamedConfiguration
        Implements IEntityTypeConfiguration(Of Entity)

        Public Sub ConfigureEntity(builder As EntityTypeBuilder(Of Entity)) Implements IEntityTypeConfiguration(Of Entity).Configure
            Dim columnNames = {"ColumnName", "ColumnName", "ColumnName", "ColumnName"} ' Compliant - model configuration
        End Sub
    End Class

    Public MustInherit Class NotADbContext
        Protected MustOverride Sub OnModelCreating(modelBuilder As Object)
    End Class

    Public Class FakeContext
        Inherits NotADbContext

        Protected Overrides Sub OnModelCreating(modelBuilder As Object)
            Dim values = {
                "NotADbContext", ' Noncompliant {{Define a constant instead of using this literal 'NotADbContext' 4 times.}}
                "NotADbContext", ' Secondary
                "NotADbContext", ' Secondary
                "NotADbContext"  ' Secondary
            }
        End Sub
    End Class

    Public MustInherit Class DbContextWithCustomOnModelCreating
        Inherits DbContext

        Protected Overloads Overridable Sub OnModelCreating(value As String)
        End Sub
    End Class

    Public Class CustomOnModelCreatingContext
        Inherits DbContextWithCustomOnModelCreating

        Protected Overrides Sub OnModelCreating(value As String)
            Dim values = {
                "CustomOverload", ' Noncompliant {{Define a constant instead of using this literal 'CustomOverload' 4 times.}}
                "CustomOverload", ' Secondary
                "CustomOverload", ' Secondary
                "CustomOverload"  ' Secondary
            }
        End Sub
    End Class
End Namespace
