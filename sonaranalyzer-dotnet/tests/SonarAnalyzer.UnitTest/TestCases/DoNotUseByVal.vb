Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class Foo
        Public Function NoByVal(parameter As String) As String ' Compliant
            Return parameter
        End Function

        Public Function OptionalNoByVal(Optional parameter As String = "Default") As String ' Compliant
            Return parameter
        End Function

        Public Function WithByVal(ByVal byValParameter As String) As String ' Noncompliant {{Remove this redundant 'ByVal' modifier.}}
'                                 ^^^^^
            Return byValParameter
        End Function

        Public Function SecondParameter(parameter As String, ByVal byValParameter As String) As String ' Noncompliant
'                                                            ^^^^^
            Return byValParameter
        End Function

        Public Function OptionalWithByVal(Optional ByVal byValParameter As String = "Default") As String ' Noncompliant
            Return byValParameter
        End Function

        Public Function NoByRef(ByRef parameter As String) As String ' Compliant
            Return parameter
        End Function

        Public Function OptionalNoByRef(Optional ByRef parameter As String = "Default") As String ' Compliant
            Return parameter
        End Function
    End Class
End Namespace
