Imports System

Namespace Tests.Diagnostics
    Public Class FunctionNestingDepth

        Public Sub New()
            Do While True
                Try
                    While True
                        If (True) Then  ' Noncompliant {{Refactor this code to not nest more than 3 control flow statements.}}
'                       ^^
                            Console.WriteLine()
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
                        Console.WriteLine()
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

        Public Sub SelectCase()
            If True Then
                Select Case 42
                    Case 0 To 41
                    Case 42
                        Select Case 42
                            Case 1
                            Case Else
                        End Select
                End Select
            End If

            If True Then
                Select Case 42
                    Case 0 To 41
                    Case 42
                        Select Case 42
                            Case 1
                                If True Then    ' Noncompliant
                                End If
                            Case Else
                                If True Then    ' Noncompliant
                                End If
                        End Select
                End Select
            End If
        End Sub


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

        Public Custom Event OnSomething As EventHandler ' Error [BC31132]
            AddHandler() ' Error [BC31133]
                If (True) Then
                    If (True) Then
                        If (True) Then

                            If (True) Then ' Noncompliant
                            End If
                        End If
                    End If
                End If
            End AddHandler
            RemoveHandler() ' Error [BC31133]
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
