Imports System
Imports System.ComponentModel.Composition

Namespace Classes
    Module Constants
        Public Const ContractName As String = "asdasd"
    End Module

    Interface MyInterface
    End Interface

    ' Noncompliant@+2 {{Implement 'MyInterface' on 'NotExported' or remove this export attribute.}}
    ' Noncompliant@+2 {{Derive from 'Exported' on 'NotExported' or remove this export attribute.}}
    <Export(GetType(MyInterface))>
    <Export(GetType(Exported))>
    Class NotExported
'    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-2
'    ^^^^^^^^^^^^^^^^^^^^^^^^^ @-2
    End Class

    ' Noncompliant@+2
    ' Noncompliant@+2
    <Export("something", GetType(MyInterface))>
    <Export(Constants.ContractName, GetType(IDisposable))>
    Class NotExported_MultipleArgs
    End Class

    <Export(GetType(MyInterface)), Export(GetType(IComparable)), Export(GetType(IDisposable))>
    Class NotExported_Multiple
'    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1 {{Implement 'MyInterface' on 'NotExported_Multiple' or remove this export attribute.}}
'                                  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-2 {{Implement 'IComparable' on 'NotExported_Multiple' or remove this export attribute.}}
        Implements IDisposable ' Error [BC30149]
        Sub Dispose()
        End Sub
    End Class

    ' Noncompliant@+1
    <ExportAttribute(GetType(MyInterface))>
    Class NotExported_FullAttributeName
    End Class

    ' Noncompliant@+2
    <Export(GetType(MyInterface))>
    <Export(GetType(Descendant))>
    Class Exported
        Implements MyInterface
    End Class

    ' Exposing ourselves
    <Export>
    <Export("something")>
    <Export(GetType(Exporting_Ourselves))>
    Class Exporting_Ourselves
    End Class

    ' Noncompliant@+1
    <InheritedExport(GetType(MyInterface))>
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

    ' Noncompliant@+1 {{Implement 'ISomething(Of BaseThing)' on 'Something(Of BaseThing)' or remove this export attribute.}}
    <Export(GetType(ISomething(Of BaseThing)))>
    Public Class Something(Of BaseThing)
    End Class

    ' Noncompliant@+1
    <Export(GetType(ISomething(Of BaseThing)))>
    Public Class SomethingImplementation
        Implements ISomething(Of BaseThing2)
    End Class

    <Export(GetType(ISomething(Of)))>
    Public Class OtherSomething(Of T)
        Implements ISomething(Of T)
    End Class

    ' Noncompliant@+2
    ' Error@+1 [BC31501]
    <Export(ContractType:=GetType(IDisposable))>
    Class Exporting_InvalidSyntax
    End Class
End Namespace
