﻿Namespace Tests.Diagnostics
    Public Class VB14

        Private Sub ConditionalAccessNullPropagation(ByVal o As Object)
            If o Is Nothing Then
                If Equals(o?.ToString(), Nothing) Then
                '         ^                               Noncompliant
                '           ^^^^^^^^^^^                   Secondary@-1
                '  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^         Noncompliant@-2
                End If
                If o?.GetHashCode() Is Nothing Then
                '  ^                                       Noncompliant
                '    ^^^^^^^^^^^^^^                        Secondary@-1
                '  ^^^^^^^^^^^^^^^^^^^^^^^^^^^             Noncompliant@-2
                End If
            End If
        End Sub

        Friend Enum MyEnum
            One
            Two
        End Enum

        Friend Class MyClassWithEnum
            Public myEnum As MyEnum
        End Class

        Public Sub EnumMemberAccess()
            Dim m = New MyClassWithEnum()
            Console.WriteLine(m.myEnum)
            m = Nothing
            If m?.myEnum = MyEnum.One Then
            '  ^                                       Noncompliant
            '    ^^^^^^^                               Secondary@-1
            '  ^^^^^^^^^^^^^^^^^^^^^^                  Noncompliant@-2
            End If
        End Sub

        Friend Class FooContainer
            Public Property Foo As Boolean
        End Class

        Friend Class TestNullConditional
            Private Sub First(ByVal fooContainer As FooContainer, ByVal bar As Boolean)
                If fooContainer?.Foo = False OrElse bar Then
                    Console.WriteLine(If(bar, "1", "2"))
                Else
                    Console.WriteLine(If(fooContainer IsNot Nothing, "3", "4"))
                End If
            End Sub

            Private Sub Second(ByVal fooContainer As FooContainer)
                If fooContainer?.Foo <> True Then
                    Console.WriteLine("3")
                    If fooContainer IsNot Nothing Then
                        Console.WriteLine("4")
                    End If
                End If
            End Sub

            Public Class Result
                Public Property Succeed As Boolean

                Public Shared Function Test() As Result
                    If Date.Now.Day = 17 Then ' swap value here to test both cases if needed
                        Return New Result()
                    End If
                    Return Nothing
                End Function
            End Class

            Public Shared Sub Compliant1()
                Dim result = TestNullConditional.Result.Test()

                If result Is Nothing OrElse Not result.Succeed Then
                    Console.WriteLine("shorted")
                    If result IsNot Nothing Then
                        Console.WriteLine("other")
                    End If
                End If

                If result?.Succeed <> True Then
                    Console.WriteLine("shorted")
                    If result IsNot Nothing Then
                        Console.WriteLine("other")
                    End If
                End If
            End Sub

            Public Shared Sub NonCompliant1()
                Dim result As Result = Nothing
                If result?.Succeed IsNot Nothing Then ' Noncompliant
                    '                                   Secondary@-1
                    '                                   Noncompliant@-2
                    Console.WriteLine("shorted") '      Secondary
                    If result IsNot Nothing Then
                        Console.WriteLine("other")
                    End If
                End If
            End Sub

            Public Shared Sub NonCompliant2()
                Dim result As Result = New Result()
                If result?.Succeed IsNot Nothing Then ' Noncompliant
                    '                                   Noncompliant@-1
                    Console.WriteLine("shorted")
                    While result IsNot Nothing         ' Noncompliant
                        Console.WriteLine("other")
                    End While
                End If
            End Sub

            Public Class A
                Public Property booleanVal As Boolean
            End Class

            Public Shared Sub Compliant2()
                Dim aObj As A = Nothing
                If If(aObj?.booleanVal, False) Then ' Noncompliant
                    '                                 Secondary@-1
                    '                                 Noncompliant@-2
                    Console.WriteLine("a")
                End If
            End Sub

            Public Shared Sub NonCompliant3()
                Dim aObj As A = Nothing
                If aObj?.booleanVal Is Nothing Then ' Noncompliant
                    '                                 Secondary@-1
                    '                                 Noncompliant@-2
                    Console.WriteLine("a")
                End If

                If aObj?.booleanVal IsNot Nothing Then ' Noncompliant
                    '                                    Secondary@-1
                    '                                    Noncompliant@-2
                    Console.WriteLine("a")             ' Secondary
                End If
            End Sub

            Public Shared Sub Compliant3(ByVal a As A)

                If a?.booleanVal = True Then
                    Console.WriteLine("Compliant")
                    Return
                End If

                If a IsNot Nothing Then  ' Compliant
                End If
            End Sub

            Public Shared Sub NonCompliant4(ByVal a As A)

                If a?.booleanVal Is Nothing Then
                    Console.WriteLine("Compliant")
                    Return
                End If

                If a IsNot Nothing Then ' Noncompliant
                End If
            End Sub

            Public Shared Sub Compliant4(ByVal a As A)
                If a?.booleanVal Is Nothing Then
                    Console.WriteLine("Compliant")
                End If

                If a IsNot Nothing Then ' Compliant
                End If
            End Sub

            Public Shared Sub Compliant5(ByVal a As A)
                While If(a?.booleanVal Is Nothing, True, False)    ' Compliant
                    Console.WriteLine("Compliant")
                End While
            End Sub

            Public Shared Sub NonCompliant5()
                Dim a As A = Nothing
                While If(a?.booleanVal Is Nothing, True, False) ' Noncompliant
                    '                                             Secondary@-1
                    '                                             Noncompliant@-2
                    '                                             Secondary@-3
                    Console.WriteLine("Compliant")
                End While
            End Sub

            Public Class S
                Public str As String = Nothing
            End Class

            Public Shared Sub Compliant6(ByVal sObj As S)
                If sObj?.str?.Length > 2 Then
                    Console.WriteLine("a")
                End If
            End Sub

            Public Shared Sub NonCompliant6()
                Dim sObj As S = Nothing
                If sObj?.str?.Length > 2 Then ' Noncompliant
                    '                           Secondary@-1
                    '                           Noncompliant@-2
                    Console.WriteLine("a")    ' Secondary
                End If
            End Sub
        End Class
    End Class
End Namespace
