Imports System
Imports System.Collections.Generic

Public Class Foo ' Noncompliant {{Move the type 'Foo' into a named namespace.}}
    Class InnerFoo ' Compliant - we want to report only on the outer class
    End Class
End Class

Public Structure Bar ' Noncompliant {{Move the type 'Bar' into a named namespace.}}
    Structure InnerBar ' Compliant - we want to report only on the outer struct
    End Structure
End Structure

Namespace Tests.Diagnostics
    Class Program
    End Class

    Structure MyStruct
    End Structure
End Namespace
