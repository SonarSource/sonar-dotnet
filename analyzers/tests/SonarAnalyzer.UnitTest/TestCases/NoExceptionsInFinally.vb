Imports System

Public Class TestCases

    Public Sub Method1()
        Try
            Throw New ArgumentException()
        Finally
            Throw New InvalidOperationException() ' Noncompliant
        End Try
    End Sub

    Public Sub Method2()
        Try
            Throw New ArgumentException()
        Finally
            Try

            Finally
                Throw New InvalidOperationException() ' Noncompliant
            End Try
        End Try
    End Sub

    Public Sub Method3()
        Try
            Throw New ArgumentException()
        Finally
            Try
                Throw New InvalidOperationException() ' Noncompliant
            Catch ex As InvalidCastException
            End Try
        End Try
    End Sub

    Public Sub Method4()
        Dim a As Integer = 0
        Try
            a += 1
            Throw New ArgumentException()
        Finally
            If a > 0 Then Throw New InvalidOperationException() ' Noncompliant
        End Try
    End Sub

    Public Sub Method5()
        Try
            Throw New ArgumentException()
        Finally
            Try
                Try
                    Throw New ArgumentOutOfRangeException() ' Noncompliant
                Finally
                    Throw New InvalidOperationException()   ' Noncompliant
                End Try
            Catch
            End Try
        End Try
    End Sub

    Public Sub Method6()
        Try
            Throw New ArgumentException()
        Finally
            Try
                Try
                    Throw New InvalidOperationException() ' Noncompliant
                Catch
                End Try
                Throw New ArgumentOutOfRangeException() ' Noncompliant
            Catch ex As ArgumentOutOfRangeException
                Throw ' Noncompliant
            End Try
        End Try
    End Sub

    Public Sub Method7()
        Try
            Throw New ArgumentException()
        Finally
            Try
                Try
                    Try
                        Throw New InvalidOperationException() ' Noncompliant
                    Catch
                    End Try
                    Throw New ArgumentOutOfRangeException() ' Noncompliant
                Catch ex As ArgumentOutOfRangeException
                    Throw ' Noncompliant
                End Try
            Catch ex As ArgumentOutOfRangeException
            End Try
        End Try
    End Sub

    Public Sub Method8()
        Try
            Throw New ArgumentException()
        Finally
        End Try
    End Sub

    Public Sub Method9()
        Try
            Throw New ArgumentException()
        Finally
            Try
                Throw New InvalidOperationException() ' Noncompliant
            Catch ex As InvalidOperationException
            End Try
        End Try
    End Sub

    Public Sub Method10()
        Try
            Throw New ArgumentException()
        Finally
            Try
                Throw New InvalidOperationException() ' Noncompliant
            Catch
            End Try
        End Try
    End Sub

    Public Sub Method11()
        Try
            Throw New ArgumentException()
        Finally
            Try
                Try
                    Try
                        Throw New InvalidOperationException() ' Noncompliant
                    Catch
                        Throw New ArgumentOutOfRangeException() ' Noncompliant
                    End Try
                Catch ex As ArgumentOutOfRangeException
                    Throw ' Noncompliant
                End Try
            Catch
            End Try
        End Try
    End Sub

End Class
