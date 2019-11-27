
Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics

    Module ParameterAssignedToStatic

        ' Error@+1 [BC30002]
        <Extension()>
        Static Sub MySub(ByVal a As Integer) ' Error [BC30242] Static is not valid
            a = 42 ' Noncompliant {{Introduce a new variable instead of reusing the parameter 'a'.}}
'           ^
            Try

            Catch exc As Exception
                exc = New Exception() ' Noncompliant
                Dim v As Integer = 5
                v = 6
                Throw exc
            End Try
        End Sub

    End Module

    Public Class ParameterAssignedTo

        Sub f00(xx As String)
            Dim tmp As Integer = xx.Length
            xx = "foo"
        End Sub

        Sub f01(x As String)
            Dim tmp As Integer = x.Length
            tmp = 5
            x = "1"
        End Sub

        Sub f02(x As Integer)
            f1(x)
            x = 1
        End Sub

        Sub f03(x As Integer)
            x += x
        End Sub

        Sub f04(x As Integer)
            x -= x
        End Sub
        Sub f05(x As Integer)
            x *= x
        End Sub

        Sub f06(x As Integer)
            x <<= x
        End Sub

        Sub f1(a As Integer)
            a = 42 ' Noncompliant
        End Sub

        Sub f2(a As Integer)
            Dim tmp As Integer = a
            tmp = 42
        End Sub

        Sub f3(ByRef a As Integer)
            a = 42
        End Sub

        Shared Sub f5()
        End Sub

        Sub f6(a As Integer, b As Integer, c As Integer, d As Integer, e As Integer)
            b = 42  ' Noncompliant
            e = 0   ' Noncompliant
        End Sub

        Delegate Sub d1(c As Integer, ByRef d As Integer)

        Private Event e As d1
        Sub f8(param As Func(Of Integer, Integer))
            Dim dd As d1 = Sub(c As Integer, ByRef d As Integer)
                               c = 0 ' Noncompliant
                               d = 0
                           End Sub
            param = Function(i) 42 ' Noncompliant

            AddHandler e, Sub(foo2 As Integer, ByRef foo3 As Integer)
                              foo2 = 0 ' Noncompliant
                              foo3 = 0
                          End Sub

            f8(Function(x)
                   Return 0
               End Function)
            f8(Function(X)
                   X = 0 ' Noncompliant
                   Return X
               End Function)
            f8(Function(X As Integer)
                   Return 0
               End Function)
            f8(Function(X As Integer)
                   X = 0 ' Noncompliant
                   Return 0
               End Function)
        End Sub

        Default Public Property Item(Index As Integer) As Integer
            Get
                Index = 1 ' Noncompliant
                Return 0
            End Get
            Set(Value As Integer)
                Index = 1  ' Noncompliant
                Value = 45 ' Noncompliant
            End Set
        End Property

        Public Event SomeEvent()

        Public Sub f9()
            AddHandler SomeEvent, Sub()
                                  End Sub
        End Sub

        Public Sub f10(Param As Func(Of Integer, Integer))
            Dim Tmp As Func(Of Integer, Integer) = Param
            Param = Function(i) 42
        End Sub

        Public Sub f11(a As Integer, b As Integer, c As Integer, d As Integer, e As Boolean)
            a += 1
            b *= 1
            c -= 1
            d -= 1
            e = Not e
        End Sub

        Public Function f12(param As Integer) As Integer
            param = param + 1
            Return param
        End Function

        Public Sub f13(x As String)
            Dim b As Boolean = x Is Nothing
            If b Then x = ""
        End Sub

        Public Sub f14(x As String)
            x = "" ' Noncompliant
        End Sub

        Public Sub f15(x As String)
            Dim y As String = ((x))
            x = ""
        End Sub

        Public Function f16(x As String) As String
            If x Is Nothing Then
                x = ""
            End If
            Return x
        End Function

        Public Function f17(text As String) As String
            If String.IsNullOrWhiteSpace(text) Then
                text = "(empty)"
            End If
            Return text
        End Function

    End Class

    Public Class ExceptionHandling

        Sub foo()
            Dim Lst As New List(Of String)
            Try

            Catch ex As Exception
                While (ex IsNot Nothing)
                    Lst.Add(Log(ex))
                    ex = ex.InnerException
                End While
            End Try
        End Sub

        Sub quix(e As Exception)
            If e IsNot Nothing Then
                Log(e)
                e = New Exception("")
            End If
        End Sub

        Sub serialized()
            Try

            Catch exSerial As Exception
                Log(exSerial)
            End Try
            Try

            Catch exSerial As Exception 'Same name As previous statement
                exSerial = New Exception("Obfuscation")    'Noncompliant
            End Try
        End Sub

        Sub nested()
            Try

            Catch exOuter As Exception
                Try
                    Log(exOuter)
                Catch exInner As Exception
                    exOuter = New Exception("Compliant")
                    exInner = exOuter  'Noncompliant
                End Try
            End Try
        End Sub

        Private Function Log(ex As Exception) As String

        End Function

    End Class

End Namespace
