Namespace Tests.Diagnostics
    Class SingleStatementPerLine
        Sub Main()
            Dim a = 0 : Dim b = 0  ' Noncompliant {{Reformat the code to have only one statement per line.}}
'           ^^^^^^^^^^^^^^^^^^^^^
        End Sub
        Sub New()
            If someCondition Then : doSomething() ' Noncompliant
            End If
            If someCondition Then : doSomething() ' Noncompliant
            Else
                doSomething()
            End If

            Dim i As Integer = 5
            i = 6 : i = 7 ' Noncompliant

            If Me.CurrentDataSource.Person.Any(Function(i) i.PersonID = ID And
                                               i.PersonCategory = 2) Then

            End If

            Dim increment1 = Function(x) x + 1 'Compliant
            Dim increment2 = Function(x)
                                 Return x + 2 'Compliant
                             End Function

            Dim increment3 = Function(x)
                                 Return x + 2 : Return x + 2 'Noncompliant
                             End Function

        End Sub
    End Class

End Namespace