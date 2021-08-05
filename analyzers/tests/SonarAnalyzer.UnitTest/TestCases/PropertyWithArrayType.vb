Module Module1

    ' Internal state
    Dim Array = {"apple", "banana", "orange", "pineapple", "strawberry"}

    Public Property NotAProblem As Byte()   'Noncompliant FP, this doens't copy anything inside

    ReadOnly Property Foo As String()   ' Noncompliant {{Refactor 'Foo' into a method, properties should not be based on arrays.}}
        '             ^^^
        Get
            Dim copy = Array.Clone      ' Expensive call
            Return copy
        End Get
    End Property

    Property Foo2() As String()         ' Noncompliant
        Get
            Dim copy = Array.Clone      ' Expensive call
            Return copy
        End Get
        Set(value As String())
        End Set
    End Property

    Property Foo3() As String
        Get
            Return "aaa"
        End Get
        Set(value As String)
        End Set
    End Property

End Module

Public Interface IBase

    ReadOnly Property Hash As Byte()    ' Noncompliant

End Interface

Public MustInherit Class Base

    Public MustOverride Property Bad As Byte()  ' Noncompliant

End Class

Public Class Sample
    Inherits Base
    Implements IBase

    Public ReadOnly Property Hash As Byte() Implements IBase.Hash   ' Noncompliant FP, this class cannot change interface signature
        Get
        End Get
    End Property

    Public Overrides Property Bad As Byte() ' Ccompliant, this class cannot change overriden property signature

End Class
