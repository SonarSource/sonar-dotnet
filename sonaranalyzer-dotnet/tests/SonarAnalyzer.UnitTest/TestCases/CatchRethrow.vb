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
            Catch __unusedArgumentException1__ As ArgumentException 'Noncompliant
                Throw
            End Try

            Try
                doSomething()
            Catch __unusedArgumentException1__ As ArgumentException 'Noncompliant
                Throw
            Catch __unusedNotSupportedException2__ As NotSupportedException 'Noncompliant
                Throw
            End Try

            Try
                doSomething()
            Catch __unusedArgumentException1__ As ArgumentException 
                Throw
            Catch
                Console.WriteLine("")
                Throw
            End Try

            Try
                doSomething()
            Catch __unusedArgumentException1__ As ArgumentException 'Noncompliant
                Throw
            Catch __unusedNotSupportedException2__ As NotSupportedException
                Console.WriteLine("")
                Throw
            End Try

            Try
                doSomething()
            Catch __unusedArgumentException1__ As ArgumentException When True
                Throw
            Catch __unusedNotSupportedException2__ As NotSupportedException
                Console.WriteLine("")
                Throw
            End Try

            Try
                doSomething()
            Catch __unusedNotSupportedException1__ As NotSupportedException 'Noncompliant
                Throw
            Finally
            End Try

            Try
                doSomething()
            Catch __unusedArgumentNullException1__ As ArgumentNullException 'Noncompliant
                Throw
            Catch __unusedNotImplementedException2__ As NotImplementedException
                Console.WriteLine("")
                Throw
            Catch __unusedArgumentException3__ As ArgumentException 'Noncompliant
                Throw
            Catch 'Noncompliant
                Throw
            End Try

            Try
                doSomething()
            Catch __unusedArgumentNullException1__ As ArgumentNullException 'Noncompliant
                Throw
            Catch __unusedNotImplementedException2__ As NotImplementedException
                Console.WriteLine("")
                Throw
            Catch __unusedSystemException4__ As SystemException 'Noncompliant
                Throw
            Catch 'Noncompliant
                Throw
            End Try
        End Sub
    End Class
End Namespace
