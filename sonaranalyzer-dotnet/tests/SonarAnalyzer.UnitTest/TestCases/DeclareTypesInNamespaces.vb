Imports System
Imports System.Collections.Generic

Public Class Foo ' Noncompliant {{Move 'Foo' into a named namespace.}}
    Class InnerFoo ' Compliant - we want to report only on the outer class
    End Class
End Class

Public Structure Bar ' Noncompliant {{Move 'Bar' into a named namespace.}}
    Structure InnerBar ' Compliant - we want to report only on the outer struct
    End Structure

    Public Enum InnerEnu ' Compliant - we want to report only on outer enum
        Test
    End Enum
End Structure

Public Interface Int ' Noncompliant {{Move 'Int' into a named namespace.}}
    Interface InnerInt ' Compliant - we want to report only on the outer interface
    End Interface
End Interface

Public Enum Enu ' Noncompliant {{Move 'Enu' into a named namespace.}}
    Test
End Enum

Namespace Tests.Diagnostics
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
