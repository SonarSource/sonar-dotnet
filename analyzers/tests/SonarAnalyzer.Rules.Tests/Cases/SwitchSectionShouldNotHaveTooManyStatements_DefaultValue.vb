Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic

Namespace Tests.TestCases
    Class Foo
        Public Sub Test()
            Dim number As Integer = 8
            Dim theCustomer As New Customer
            Dim thisLock As New Object

            Select Case number
                Case 1 To 5 ' Noncompliant {{Reduce this 'Case' block number of statements from 9 to at most 8, for example by extracting code into a procedure.}}
'               ^^^^^^^^^^^
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                    Test()
                Case 6 ' Noncompliant {{Reduce this 'Case' block number of statements from 20 to at most 8, for example by extracting code into a procedure.}}
                    If False                                                 ' +1
                    ElseIf True                                              ' +1
                    Else                                                     ' +1
                        For index = 1 To 10                                  ' +1
                        Next
                        While True                                           ' +1
                        End While
                        Do
                        Loop While (number < 1)                              ' +1
                        Do
                        Loop Until (number = 1)                              ' +1
                        Select Case number                                   ' +1
                            Case 1                                           ' +1
                            Case 2                                           ' +1
                            Case Else                                        ' +1
                        End Select
                        If True Then Test()                                  ' +1
                        Try                                                  ' +1
                        Catch ex As Exception                                ' +1
                        Finally                                              ' +1
                        End Try
                    End If
                    With theCustomer                                         ' +1
                        .Name = "d"                                          ' +1
                        .City = "foo"                                        ' +1
                    End With
                    Using writer As TextWriter = File.CreateText("log.txt")  ' +1
                    End Using
                    SyncLock thisLock                                        ' +1
                    End SyncLock
                Case Else
                    Test()
            End Select
		End Sub
    End Class

    Public Class Customer
        Public Property Name As String
        Public Property City As String
        Public Property URL As String

        Public Property Comments As New List(Of String)
    End Class
End Namespace
