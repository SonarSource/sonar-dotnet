Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases
    Class UseWithStatement

        Property X As Integer

        Sub Main()
            Dim product As Product
            product.X.Name = ""           ' Noncompliant {{Wrap this and the following 3 statements that use 'product.X' in a 'With' statement.}}
            product.X.RetailPrice += 0
            X = product.X.WholesalePrice
            product.X.ToString()

            Int32.Parse("1")
            Int32.Parse("2")


            Me.X.ToString() ' Compliant
            If Not Me.X.ToString() = "" Then Exit Sub

            Me.X.ToString() ' Compliant

            Me.ToString() ' Compliant, single element access
            Me.ToString()
            Me.ToString()

            x.ToString ' Compliant, single element access
            x.ToString
            x.ToString

        End Sub
    End Class

    Class Product
        Public Property X As Product
        Public Property Name As String
        Public Property RetailPrice As Integer
        Public Property WholesalePrice As Integer
    End Class
End Namespace

