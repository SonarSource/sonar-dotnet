Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
    Class RedundantNullCheck
        Private a As Object
        Private b As Object

        Public Function TestRedundantNullCheck() As Object
            If (a IsNot Nothing And TypeOf a Is RedundantNullCheck) Then ' Noncompliant {{Remove this unnecessary null check; 'TypeOf ... Is' returns false for nulls.}}
'               ^^^^^^^^^^^^^^^
            End If

            If (Not a Is Nothing And TypeOf a Is RedundantNullCheck) Then ' Noncompliant
            End If

            If (a IsNot Nothing AndAlso TypeOf a Is RedundantNullCheck) Then ' Noncompliant
            End If

            If (TypeOf a Is RedundantNullCheck And a IsNot Nothing) Then ' Noncompliant
'                                                  ^^^^^^^^^^^^^^^
            End If

            If (TypeOf a Is RedundantNullCheck And a IsNot Nothing And b IsNot Nothing) Then ' Noncompliant
'                                                  ^^^^^^^^^^^^^^^
            End If

            If (((a) IsNot Nothing) And ((TypeOf (a) Is RedundantNullCheck))) Then ' Noncompliant
            End If

            If (a IsNot Nothing Or TypeOf a Is RedundantNullCheck) Then ' Compliant - not AND operator
            End If

            If (a IsNot Nothing And TypeOf a IsNot RedundantNullCheck) Then ' Compliant
            End If

            If (a IsNot Nothing And Not TypeOf a Is RedundantNullCheck) Then ' Compliant
            End If

            If (a IsNot Nothing And TypeOf b Is RedundantNullCheck) Then ' Compliant
            End If

            If (b IsNot Nothing And TypeOf a Is RedundantNullCheck) Then ' Compliant
            End If

            If (a IsNot Nothing And a IsNot Nothing) Then ' Compliant - not related to this rule
            End If
        End Function

        Public Function TestRedundantInvertedNullCheck() As Object
            If (a Is Nothing Or Not TypeOf a Is RedundantNullCheck) Then ' Noncompliant {{Remove this unnecessary null check; 'TypeOf ... Is' returns false for nulls.}}
'               ^^^^^^^^^^^^
            End If

            If (a Is Nothing OrElse Not TypeOf a Is RedundantNullCheck) Then ' Noncompliant
            End If

            If (a Is Nothing Or TypeOf a IsNot RedundantNullCheck) Then ' Noncompliant
            End If

            If (TypeOf a IsNot RedundantNullCheck Or a Is Nothing) Then ' Noncompliant
'                                                    ^^^^^^^^^^^^

            End If

            If ((TypeOf (a) IsNot RedundantNullCheck) Or ((a) Is Nothing)) Then ' Noncompliant

            End If

            If (a Is Nothing Or Not TypeOf b Is RedundantNullCheck) Then ' Compliant
            End If

            If (b Is Nothing Or Not TypeOf a Is RedundantNullCheck) Then ' Compliant
            End If

            If (a Is Nothing Or a Is Nothing) Then ' Compliant - not related to this rule 
            End If
        End Function
    End Class
End Namespace
