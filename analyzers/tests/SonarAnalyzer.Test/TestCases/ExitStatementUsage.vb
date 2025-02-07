Imports System
Imports System.Diagnostics

Class MyClass1
    Public Sub MySub(condition As Boolean)
        If condition Then
            Exit Sub                  ' Noncompliant {{Remove this 'Exit' statement.}}
'           ^^^^^^^^
        End If
        If condition Then
            Return
        End If

        Dim number As Integer = 2
        Select Case number
            Case 1 To 5
                Exit Select           ' Compliant, not in scope of this rule
                Debug.WriteLine("Between 1 and 5, inclusive")
        End Select

        For index = 1 To 10
            If index = 5 Then
                Exit For            ' Compliant, not in scope of this rule. S1227 about "break;" was deprecated
            End If

            Do
                Do While True
                    Exit Do         ' Compliant, not in scope of this rule. S1227 about "break;" was deprecated
                Loop

                Exit Do             ' Compliant, not in scope of this rule. S1227 about "break;" was deprecated
            Loop
            ' ...
        Next
    End Sub

    Function MyFunction() As Object
        ' ...
        MyFunction = 42
        Exit Function              ' Noncompliant
    End Function

    Private newPropertyValue As String
    Public Property NewProperty() As String
        Get
            Exit Property          ' Noncompliant
            Return newPropertyValue
        End Get
        Set(ByVal value As String)
            Try
                Exit Try           ' Compliant, not in scope of this rule. S1227 about "break;" was deprecated
            Catch ex As Exception

            End Try
            newPropertyValue = value
        End Set
    End Property
End Class
