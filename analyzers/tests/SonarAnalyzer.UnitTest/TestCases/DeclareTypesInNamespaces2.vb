Imports System
Imports System.Collections.Generic

Public Class Foo2 ' Noncompliant
    Class InnerFoo ' Compliant - we want to report only on the outer class
    End Class
End Class

Public Structure Bar2 ' Noncompliant
    Structure InnerBar ' Compliant - we want to report only on the outer struct
    End Structure

    Public Enum InnerEnu ' Compliant - we want to report only on outer enum
        Test
    End Enum
End Structure

Public Interface Int2 ' Noncompliant
    Interface InnerInt ' Compliant - we want to report only on the outer interface
    End Interface
End Interface

Public Enum Enu2 ' Noncompliant
    Test
End Enum

Namespace Tests.Diagnostics2
    Class Program
    End Class

    Structure MyStruct
    End Structure

    Public Enum MyEnu
        Test
    End Enum

    Public Interface MyInt
    End Interface
End Namespace
