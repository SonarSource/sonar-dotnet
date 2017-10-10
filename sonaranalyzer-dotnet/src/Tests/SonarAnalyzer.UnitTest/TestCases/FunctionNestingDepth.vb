Imports System

Namespace Tests.Diagnostics
    Public Class FunctionNestingDepth

        Public Sub New()
            Do While True
                Try
                    While True
                        If (True) Then  ' Noncompliant {{Refactor this code to not nest more than 3 control flow statements.}}
'                       ^^
                            Console.Write()
                        End If
                    End While

                Catch

                End Try
            Loop
        End Sub

        Protected Overrides Sub Finalize()
            Do While True
                Try
                    While True
                        For Each x In {1, 2, 3} ' Noncompliant {{Refactor this code to not nest more than 3 control flow statements.}}
                        Next
                    End While
                Catch
                End Try
            Loop
        End Sub

        Public Sub M1()
            If True Then
                While True
                    Try
                    Catch
                    End Try
                End While

                If (True) Then

                ElseIf (True) Then
                    If (True) Then
                        Console.Write()
                    End If

                ElseIf (True) Then
                    Do
                        Do ' Noncompliant
                        Loop
                    Loop

                ElseIf (True) Then
                End If
            End If
        End Sub

        Public Sub M2()
            If True Then
                If (True) Then
                    If (True) Then
                        For i As Integer = 0 To 1 Step 0 ' Noncompliant

                        Next
                    End If
                End If
            End If
        End Sub

        Public Function M3() As Integer
            Return (New Func(Of Integer)(Function() As Integer
                                             If (True) Then
                                                 If (True) Then
                                                     If (True) Then

                                                         If (True) Then ' Noncompliant
                                                             Return 0
                                                         End If
                                                     End If
                                                 End If
                                             End If

                                             Return 42

                                         End Function))()
        End Function


        Public Property MyProperty As Integer
            Get
                If (True) Then
                    If (True) Then
                        If (True) Then

                            If (True) Then ' Noncompliant
                                Return 0
                            End If
                        End If
                    End If
                End If

                Return 42
            End Get

            Set
                If (True) Then
                    If (True) Then
                        If (True) Then

                            If (True) Then ' Noncompliant
                            End If
                        End If
                    End If
                End If
            End Set
        End Property

        Public Custom Event OnSomething As EventHandler
            AddHandler()
                If (True) Then
                    If (True) Then
                        If (True) Then

                            If (True) Then ' Noncompliant
                            End If
                        End If
                    End If
                End If
            End AddHandler
            RemoveHandler()
                If (True) Then
                    If (True) Then
                        If (True) Then

                            If (True) Then ' Noncompliant
                            End If
                        End If
                    End If
                End If
            End RemoveHandler
        End Event

        Public Shared Operator +(ByVal h1 As FunctionNestingDepth,
                             ByVal h2 As FunctionNestingDepth) As FunctionNestingDepth
            If (True) Then
                If (True) Then
                    If (True) Then

                        If (True) Then ' Noncompliant
                        End If
                    End If
                End If
            End If
            Return New FunctionNestingDepth
        End Operator

        Public Sub M5()
            If True Then
                If (True) Then
                    If (True) Then
                        For i As Integer = 0 To 1 Step 0 ' Noncompliant
                            If (True) Then ' Compliant

                            End If
                        Next
                    End If
                End If
            End If
        End Sub
    End Class
End Namespace
