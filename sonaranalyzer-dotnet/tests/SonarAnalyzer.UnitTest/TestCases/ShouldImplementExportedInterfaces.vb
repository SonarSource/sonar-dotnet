Imports System
Imports System.ComponentModel.Composition

Namespace Classes
    Module Constants
        Public Const ContractName As String = "asdasd"
    End Module

    Interface MyInterface
    End Interface

    <Export(GetType(MyInterface))> ' Noncompliant {{Implement 'MyInterface' on 'NotExported' or remove this export attribute.}}
    <Export(GetType(Exported))> ' Noncompliant {{Derive from 'Exported' on 'NotExported' or remove this export attribute.}}
    Class NotExported
'    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-2
'    ^^^^^^^^^^^^^^^^^^^^^^^^^ @-2
    End Class

    <Export("something", GetType(MyInterface))> ' Noncompliant
    <Export(Constants.ContractName, GetType(IDisposable))> ' Noncompliant
    class NotExported_MultipleArgs
    End Class

    <Export(GetType(MyInterface)), Export(GetType(IComparable)), Export(GetType(IDisposable))>
    Class NotExported_Multiple
'    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1 {{Implement 'MyInterface' on 'NotExported_Multiple' or remove this export attribute.}}
'                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-2 {{Implement 'IComparable' on 'NotExported_Multiple' or remove this export attribute.}}
        Implements IDisposable ' Error [BC30149]
        Sub Dispose()
        End Sub
    End Class

    <ExportAttribute(GetType(MyInterface))> ' Noncompliant
    Class NotExported_FullAttributeName
    End Class

    <Export(GetType(MyInterface))>
    <Export(GetType(Descendant))> ' Noncompliant
    Class Exported
        Implements MyInterface
    End Class

    <Export>
    <Export("something")> ' Exposing ourselves
    <Export(GetType(Exporting_Ourselves))>
    Class Exporting_Ourselves
    End Class

    <InheritedExport(GetType(MyInterface))> ' Noncompliant
    <InheritedExport(GetType(OtherAttributes))>
    Class OtherAttributes
    End Class

    <Export(GetType(MyInterface))>
    <Export(GetType(Exported))>
    class Descendant
        Inherits Exported
    End Class

    Class ExportingMembers_Are_Ignored
        <Export(GetType(MyInterface))>
        <Export(GetType(Exported))>
        Public Property MyProperty As NotExported

        <Export(GetType(MyInterface))>
        <Export(GetType(Exported))>
        Public MyField As NotExported

        <Export(GetType(MyInterface))>
        <Export(GetType(Exported))>
        Public Function MyMethod() As NotExported
        End Function
    End Class

    Interface ISomething(Of T)
    End Interface

    Public Class BaseThing
    End Class
    Public class BaseThing2
    End Class

    <Export(GetType(ISomething(Of BaseThing)))>
    Public Class BaseSomethingImplementation
        Implements ISomething(Of BaseThing)
    End Class

    <Export(GetType(ISomething(Of BaseThing)))> ' Noncompliant {{Implement 'ISomething(Of BaseThing)' on 'Something(Of BaseThing)' or remove this export attribute.}}
    Public Class Something(Of BaseThing)
    End Class

    <Export(GetType(ISomething(Of BaseThing)))> ' Noncompliant
    public Class SomethingImplementation
        Implements ISomething(Of BaseThing2)
    End Class

    <Export(GetType(ISomething(Of)))> ' Noncompliant
    Public Class OtherSomething(Of T)
        Implements ISomething(Of T)
    End Class
End Namespace
