Imports System


    <Obsolete> Class Noncompliant ' Noncompliant {{Add an explanation.}}
'   ^^^^^^^^^^
        <Obsolete> ' Noncompliant 
        Enum [Enum]
            foo
            bar
        End Enum

        <Obsolete> ' Noncompliant 
        Private Sub New()
        End Sub

        <Obsolete> ' Noncompliant 
        Private Sub Method()
        End Sub

        <Obsolete> ' Noncompliant 
        Private Function Func() As Integer
            Return 42
        End Function

        <Obsolete> ' Noncompliant 
        Private Property [Property] As Integer
        <Obsolete> ' Noncompliant 
        Private Field As Integer
        <Obsolete> ' Noncompliant 
        Private CustomEvent As EventHandler
        <Obsolete> ' Noncompliant 
        Delegate Sub [Delegate]()
    End Class

    <Obsolete> ' Noncompliant 
    Interface IInterface
        <Obsolete> ' Noncompliant 
        Sub Method()
    End Interface

    <Obsolete> ' Noncompliant 
    Structure ProgramStruct
        <Obsolete> ' Noncompliant 
        Private Sub Method()
        End Sub
    End Structure

    <Obsolete("explanation")>
    Class Compliant
        <Obsolete("explanation")>
        Enum [Enum]
            foo
            bar
        End Enum

        <Obsolete("explanation")>
        Private Sub New()
        End Sub

        <Obsolete("explanation")>
        Private Sub Method()
        End Sub

        <Obsolete("explanation")>
        Private Property [Property] As String
        <Obsolete("explanation", True)>
        Private Field As Integer
        <Obsolete("explanation", False)>
        Private CustomEvent As EventHandler
        <Obsolete("explanation")>
        Delegate Sub [Delegate]()
    End Class

    <Obsolete("explanation")>
    Interface IInterface_Explained
        <Obsolete("explanation")>
        Sub Method()
    End Interface

    <Obsolete("explanation")>
    Structure ProgramStruct_Explained
        <Obsolete("explanation")>
        Private Sub Method()
        End Sub
    End Structure

    Class NoAttributes
        <CLSCompliant(False)>
        Enum [Enum]
            foo
            bar
        End Enum

        Private Sub New()
        End Sub

        Private Sub Method()
        End Sub

        Private Property [Property] As Integer
        Private Field As Integer
        Private CustomEvent As EventHandler
        Delegate Sub [Delegate]()
    End Class
End Namespace
