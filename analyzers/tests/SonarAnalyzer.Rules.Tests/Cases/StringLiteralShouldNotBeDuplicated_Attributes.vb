Imports System
Imports System.Diagnostics
Imports System.Diagnostics.CodeAnalysis

' compliant - outside class and in attribute -> ignored
<Assembly: DebuggerDisplay("foo", Name:="foo", TargetTypeName:="foo")>

Namespace Tests.Diagnostics
    Public Class ConstantsInAttributesShouldBeIgnored
        <SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")> ' Compliant - ignored completely
        Private field1 As String

        <SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")>
        Private field2 As String

        <SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")>
        Private field3 As String

        ' Compliant - repetition below threshold - string in attributes are not counted
        Private values As String() = { "Microsoft.Design", "Microsoft.Design" }

        ' nonCompliant - repetition above threshold. String in attributes should not be highlighted
        Private values2 As String() = { "CA1024:UsePropertiesWhereAppropriate", ' Noncompliant {{Define a constant instead of using this literal 'CA1024:UsePropertiesWhereAppropriate' 3 times.}}
            "CA1024:UsePropertiesWhereAppropriate",                             ' Secondary
            "CA1024:UsePropertiesWhereAppropriate" }                            ' Secondary

    End Class
    
    <DebuggerDisplay("12345", Name := "12345", TargetTypeName := "12345")> ' Compliant - in attribute -> ignored
    Public Class Class1

        <DebuggerDisplay("12345", Name := "12345", TargetTypeName := "12345")>
        Private field1 As String = "12345" ' Noncompliant {{Define a constant instead of using this literal '12345' 4 times.}}

        <DebuggerDisplay("12345", Name := "12345", TargetTypeName := "12345")>
        Private ReadOnly Property Name As String = "12345"
        '                                          ^^^^^^^ Secondary

        <DebuggerStepThrough>
        Public Function DoStuff(Optional arg As String = "12345")
        '                                                ^^^^^^^ Secondary
            If arg = "12345" Then
        '            ^^^^^^^ Secondary
                Return True
            End If

            Return False

        End Function

    End Class

End Namespace
