Public Interface IBase
End Interface

Public Interface INotImplemented
End Interface

Public Interface INotImplementedWithBase
    Inherits IBase

End Interface

Public Interface IImplemented
End Interface

Public Interface IGeneric(Of T)
End Interface

Public Interface IGeneric(Of TFirst, TSecond)
End Interface

Public Class ImplementerOfIBase
    Implements IBase

End Class

Public Class ImplementerOfIImplemented
    Implements IImplemented

End Class

Public Class ImplementerOfIIGenericInt
    Implements IGeneric(Of Integer)

End Class

Public Class ImplementerOfIIGenericString
    Implements IGeneric(Of String)

End Class

Public Class ImplementerOfIIGenericStringObject
    Implements IGeneric(Of String, Object)

End Class

Public Class ImplementerOfIIGenericStringString
    Implements IGeneric(Of String, String)

End Class

Public Class ImplementerOfIIGenericIntString
    Implements IGeneric(Of Integer, String)

End Class

Public Class EmptyClass
End Class

Public Class EmptyBase
End Class

Public Class InheritsAndImplements
    Inherits EmptyBase
    Implements IBase

End Class

Public Class InvalidCastToInterface

    Public Class Nested
        Inherits EmptyClass
        Implements IDisposable

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub

    End Class

    Public Sub Main()
        Dim Empty As New EmptyClass(), WithDirect, WithTry, WithCType As IBase
        WithDirect = DirectCast(Empty, IBase)   ' Noncompliant {{Review this cast; in this project there's no type that extends 'EmptyClass' and implements 'IBase'.}}
        '                              ^^^^^
        WithCType = CType(Empty, IBase)         ' Noncompliant {{Review this cast; in this project there's no type that extends 'EmptyClass' and implements 'IBase'.}}
        '                        ^^^^^
        Dim A As IImplemented = DirectCast(WithDirect, IImplemented) ' Noncompliant {{Review this cast; in this project there's no type that implements both 'IBase' and 'IImplemented'.}}
        WithTry = TryCast(Empty, IBase)         ' Compliant, will return Nothing
        Dim B As Boolean = TypeOf Empty Is IBase

        Dim Arr As EmptyClass() = {}
        Dim Arr2 As IBase() = DirectCast(Arr, IBase())

        Dim EmptyBase As New EmptyBase()
        Dim y As IBase = DirectCast(EmptyBase, IBase)

        Dim i As IBase = New InheritsAndImplements()
        Dim c As INotImplemented = DirectCast(i, INotImplemented) ' Compliant, because INotImplemented doesn't have concrete implementation
        Dim d As INotImplemented = DirectCast(i, INotImplemented) ' Compliant
        Dim e As INotImplemented = DirectCast(i, INotImplementedWithBase)

        Dim O As Object = True
        e = DirectCast(O, INotImplementedWithBase)

        Dim Z As IDisposable = DirectCast(New EmptyClass(), IDisposable)
    End Sub

    Public Sub Generics()
        Dim List As New List(Of Integer)
        Dim IEnumerable = DirectCast(List, IEnumerable(Of Integer))
        Dim IList = DirectCast(List, IList)
        Dim ICollection = DirectCast(List, ICollection)

        Dim FromString As New ImplementerOfIIGenericString()
        Dim ToString As IGeneric(Of String) = DirectCast(FromString, IGeneric(Of String))
        Dim ToInt As IGeneric(Of Integer) = DirectCast(FromString, IGeneric(Of Integer))    ' Noncompliant

        Dim FromStringObject As New ImplementerOfIIGenericStringObject()
        Dim ToStringObject As IGeneric(Of String, Object) = DirectCast(FromStringObject, IGeneric(Of String, Object))
        Dim ToStringString As IGeneric(Of String, String) = DirectCast(FromStringObject, IGeneric(Of String, String))   ' Noncompliant
        Dim ToIntString As IGeneric(Of Integer, String) = DirectCast(FromStringObject, IGeneric(Of Integer, String))    ' Noncompliant
        Dim ToSingleTyped As IGeneric(Of Integer) = DirectCast(FromStringObject, IGeneric(Of Integer))                  ' Noncompliant
    End Sub

    Public Sub Dynamic(D)
        D.Whatever = 42 ' Dynamic with Option Strict Off
        Dim B As IBase = DirectCast(D, IBase)
    End Sub

    Public Sub Nullable()
        Dim i? As Integer = Nothing
        Dim o As Object = i
        Dim ii = DirectCast(o, Integer) ' Compliant, this Is handled by S3655
    End Sub

End Class

Interface IFoo
End Interface

Interface IBar
End Interface

Public Class Foo
    Implements IFoo

End Class

Public Class Bar
    Implements IBar

End Class

Public Class FooBar
    Implements IFoo, IBar

End Class

Public NotInheritable Class FinalBar
    Implements IBar

End Class

Public Class Other

    Public Sub Method(Of T As New)(Generic As T)
        Dim IFoo As IFoo
        Dim IBar As IBar
        Dim Foo As Foo
        Dim Bar As Bar
        Dim FooBar As FooBar
        Dim FinalBar As FinalBar
        Dim o As Object

        o = DirectCast(Bar, IFoo)       ' Noncompliant
        o = DirectCast(IBar, IFoo)
        o = DirectCast(Bar, Foo)        ' Compliant causes compiler error ' Error   [BC30311] - invalid cast
        o = DirectCast(IBar, Foo)
        o = DirectCast(FinalBar, IFoo)  ' Compliant causes compiler error ' Warning [BC42322] - invalid cast
        o = DirectCast(Generic, Bar)    ' Compliant causes compiler error ' Error   [BC30311] - invalid cast

        o = TryCast(Bar, IFoo)
        o = TryCast(IBar, IFoo)
        o = TryCast(IBar, Foo)
        o = TryCast(Generic, Bar)

        o = TypeOf Bar Is IFoo
        o = TypeOf IBar Is IFoo
        o = TypeOf Bar Is Foo       ' Error [BC31430]	Expression Of type 'Bar' can never be of type 'Foo'
        o = TypeOf IBar Is Foo
        o = TypeOf FinalBar Is IFoo
        o = TypeOf Generic Is Bar
    End Sub

End Class
