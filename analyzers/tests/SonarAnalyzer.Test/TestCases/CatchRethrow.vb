Imports System

Namespace Tests.TestCases
    Class CatchRethrow
        Private Sub doSomething()
            Throw New NotSupportedException()
        End Sub

        Public Sub Test()
            Dim someWronglyFormatted = 45

            Try
                doSomething()
            Catch exc As Exception 'Noncompliant
                Throw
            End Try

            Try
                doSomething()
            Catch exc As ArgumentException 'Noncompliant
                Throw
            End Try

            Try
                doSomething()
            Catch exc As ArgumentException 'Noncompliant
                Throw
            Catch exc As NotSupportedException 'Noncompliant
                Throw
            End Try

            Try
                doSomething()
            Catch exc As ArgumentException
                Throw
            Catch
                Console.WriteLine("")
                Throw
            End Try

            Try
                doSomething()
            Catch exc As ArgumentException 'Noncompliant
                Throw
            Catch exc As NotSupportedException
                Console.WriteLine("")
                Throw
            End Try

            Try
                doSomething()
            Catch exc As ArgumentException When True
                Throw
            Catch exc As NotSupportedException
                Console.WriteLine("")
                Throw
            End Try

            Try
                doSomething()
            Catch exc As ArgumentException When True
                Throw
            End Try

            Try
                doSomething()
            Catch exc As NotSupportedException 'Noncompliant
                Throw
            Finally
            End Try

            Try
                doSomething()
            Catch exc As ArgumentNullException 'Noncompliant
                Throw
            Catch exc As NotImplementedException
                Console.WriteLine("")
                Throw
            Catch exc As ArgumentException 'Noncompliant
                Throw
            Catch 'Noncompliant
                Throw
            End Try

            Try
                doSomething()
            Catch exc As ArgumentNullException 'Noncompliant
                Throw
            Catch exc As NotImplementedException
                Console.WriteLine("")
                Throw
            Catch exc As SystemException 'Noncompliant
                Throw
            Catch 'Noncompliant
                Throw
            End Try
        End Sub
    End Class

    ' Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/8199
    Public Class Repro8163
        Public Sub SomeMethod()
            Throw New NotSupportedException()
        End Sub

        Public Function LogException(ex As Exception) As Boolean
            Return False
        End Function

        Public Sub CatchWithFilter()
            Try
                SomeMethod()
            Catch ex As Exception When LogException(ex)
                Throw
            End Try
        End Sub
    End Class
End Namespace
