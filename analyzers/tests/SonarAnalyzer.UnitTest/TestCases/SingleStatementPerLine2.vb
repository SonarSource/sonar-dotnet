Namespace AppendedNamespaceForConcurrencyTest.Tests.Diagnostics
    Class SingleStatementPerLine
        Sub Main()
            Dim a = 0 : Dim b = 0  ' Noncompliant {{Reformat the code to have only one statement per line.}}
'           ^^^^^^^^^^^^^^^^^^^^^

            Dim message As String = "Error in {0}.{1}.{2} : {3} Authorization error : {4}   text" : Dim number As Integer = 8
            ' Do not report on the previous line as it contains the VBC error pattern
        End Sub
        Sub New(someCondition As Boolean)
            If someCondition Then : doSomething() ' Noncompliant
            End If
            If someCondition Then : doSomething() ' Noncompliant
            Else
                doSomething()
            End If

            Dim i As Integer = 5
            i = 6 : i = 7 ' Noncompliant

            Dim increment1 = Function(x) x + 1 'Compliant
            Dim increment2 = Function(x)
                                 Return x + 2 'Compliant
                             End Function

            Dim increment3 = Function(x)
                                 Return x + 2 : Return x + 2 'Noncompliant
                             End Function

        End Sub

        Sub doSomething()
        End Sub

        Sub Test()
            Dim test As TestEnum = TestEnum.Value1
            Select Case test
                Case TestEnum.Value1
                    test = TestEnum.Value1 : test = TestEnum.Value2 ' Noncompliant
                Case TestEnum.Value2
                    ' action2
                Case Else
                    ' else
            End Select
        End Sub

        Public Enum TestEnum
            Value1
            Value2
        End Enum
    End Class

End Namespace
