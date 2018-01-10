Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace Tests.TestCases
    Class UseWithStatement

        Sub Main()
            product.X.Name = ""           ' Noncompliant {{Wrap this and the following 3 statements that use 'product.X' in a 'With' statement.}}
            product.X.RetailPrice += 0
            X = product.X.WholesalePrice
            product.X.ToString()

            Int32.Parse("1")
            Int32.Parse("2")

            Me.X.ToString() ' Compliant
            Throw Me.X.Exception

            Me.X.ToString() ' Compliant
            If Not Me.X.ToString() = "" Then Exit Sub

            Me.X.ToString() ' Compliant
            For Each x In Me.X.List
            Next

            Me.ToString() ' Compliant, single element access
            Me.ToString()
            Me.ToString()

            x.ToString ' Compliant, single element access
            x.ToString
            x.ToString

            With Me
                .Equals(x.y.z)
                .Equals(x.y.zz)
                .Equals(x.y.zzz)
            End With

            a.b.c.d.ToString ' Noncompliant {{Wrap this and the following 2 statements that use 'a.b.c' in a 'With' statement.}}
            a.b.c.e.ToString
            a.b.c.f.g.ToString

            b.a.c.d.ToString ' Noncompliant {{Wrap this and the following 1 statement that use 'b.a.c' in a 'With' statement.}}
            b.a.c.e.ToString
            b.a.d.f.g.ToString

        End Sub
    End Class
End Namespace

