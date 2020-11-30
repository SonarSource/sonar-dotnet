Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace Tests.TestCases
  Class Foo
    Public Sub Test()
      Dim target As Integer = -5
      Dim num As Integer = 3

      target =- num ' Noncompliant {{Was '-=' meant instead?}}
'             ^
      target =+ num ' Noncompliant {{Was '+=' meant instead?}}
'             ^
      target =-     num ' Noncompliant
      target=- num ' Noncompliant

      target = - num
      target = -num ' Compliant intent To assign inverse value Of num Is clear
      target =-num ' Compliant most probably intent To assign inverse value?
      target=-num ' Compliant most probably intent To assign inverse value?

      target +=- num

      target += num

      target += -num
      target = _
          +num

    End Sub
  End Class
End Namespace
