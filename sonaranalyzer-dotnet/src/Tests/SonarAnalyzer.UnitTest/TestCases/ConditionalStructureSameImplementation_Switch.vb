Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases

    Class ConditionalStructureSameCondition_Switch
        Public Sub Test()

            Select (i)

                Case 1
                    doSomething(prop)
                case 2
                    doSomethingDifferent()
                case 3
                    doSomething(prop) ' Compliant, single line
                Case 4
                        doSomething2()
                        doSomething2()


                case 5

                    'some comment here and there
                    doSomething2() ' Noncompliant {{Either merge this case with the identical one on line 21 or change one of the implementations.}}
                    doSomething2()

                Case Else
                    doSomething(prop) ' Compliant, single line

            End Select
        End Sub
    End Class
End Namespace
