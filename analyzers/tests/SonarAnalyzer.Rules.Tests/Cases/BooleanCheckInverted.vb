Imports System
Imports System.Collections.Generic

Namespace Tests.Diagnostics
    Public Class BooleanCheckInverted
        Public Sub Test()
            Dim a = 2

            If Not ((a = 2)) Then 'Noncompliant {{Use the opposite operator ('<>') instead.}}
'              ^^^^^^^^^^^^^
            End If

            Dim b As Boolean = Not (a < 10) 'Noncompliant
'                              ^^^^^^^^^^^^
            b = Not (a <= 10) 'Noncompliant
            b = Not (a > 10) 'Noncompliant
            b = Not (a >= 10) 'Noncompliant
            b = Not (a = 10) 'Noncompliant
            b = Not (a <> 10) 'Noncompliant

            If a <> 2 Then
            End If

            b = (a >= 10)

            Dim c = True AndAlso Not (New Integer(-1) {}.Length = 0) 'Noncompliant

            Dim args As Integer() = {}
            Dim d = Not Not (args.Length = 0) 'Noncompliant

            SomeFunc(Not (a >= 10)) 'Noncompliant
        End Sub

        Public Sub TestNullables()
            Dim a As Integer? = 5
            Dim b As Boolean = Not (a < 5) 'Compliant
            b = Not (a <= 5) 'Compliant
            b = Not (a > 5) 'Compliant
            b = Not (a >= 5) 'Compliant
            b = Not (a = 5) 'Noncompliant
            b = Not (a <> 5) 'Noncompliant
        End Sub

        Public Shared Sub SomeFunc(ByVal x As Boolean)
        End Sub

        Public Shared Operator =(ByVal a As BooleanCheckInverted, ByVal b As BooleanCheckInverted) As Boolean
            Return False
        End Operator

        Public Shared Operator <>(ByVal a As BooleanCheckInverted, ByVal b As BooleanCheckInverted) As Boolean
            Return Not (a = b) 'Compliant
        End Operator

        Public Shared Function IsNullOrEmpty(Of T)(ByVal collection As IList(Of T)) As Boolean
            Return Not (collection?.Count > 0) 'Compliant - not the same as "collection?.Count <= 0"
        End Function

        Public Shared Function IsNullOrEmpty1(ByVal collection As IList(Of Integer)) As Boolean
            Return Not (collection?(0) > 0) 'Compliant - not the same as "collection?(0) <= 0"
        End Function

        Public Shared Function IsNullOrEmpty2(Of T)(ByVal collection As IList(Of T)) As Boolean
            Return Not (0 < collection?.Count) 'Compliant - not the same as "collection?.Count <= 0"
        End Function

        Public Shared Function IsNullOrEmpty3(Of T)(ByVal collection As IList(Of T)) As Boolean
            Return Not (((0)) < ((collection?.Count))) 'Compliant - not the same as "collection?.Count <= 0"
        End Function

        Public Shared Function IsNullOrEmpty4(Of T)(ByVal collection As IList(Of T)) As Boolean
            Return Not (0 = collection?.Count) 'Noncompliant
        End Function
    End Class
End Namespace
