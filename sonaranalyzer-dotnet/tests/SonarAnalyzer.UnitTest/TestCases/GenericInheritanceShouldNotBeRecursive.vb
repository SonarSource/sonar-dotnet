Imports System

Namespace Tests.ClassRecursion

    ' some base classes
    Class CA(Of T)
    End Class

    Class CB(Of T)
    End Class

    Class CC(Of T1, T2)
    End Class

    Class C0(Of T)
        Inherits CA(Of C0(Of T))
    End Class

    Class C1(Of T) ' Compliant
        Inherits CA(Of CB(Of C1 (Of T)))
    End Class

    Class C2(Of T) ' Noncompliant {{Refactor this Class so that the generic inheritance chain is not recursive.}}
'         ^^
        Inherits CA(Of C2(Of CB(Of T)))
    End Class

    Class C3(Of T) ' Noncompliant
        Inherits CA(Of C3(Of C3(Of T)))
    End Class

    Class C4(Of T) ' Noncompliant
        Inherits CA(Of C4(Of CA(Of T)))
    End Class

    Class C5(Of T) ' Noncompliant
        Inherits CC(Of C5(Of CA(Of T)), CB(Of T))
    End Class

    Class C6(Of T) ' Compliant
        Inherits CC(Of C5(Of CA(Of T)), CB(Of T))
    End Class

    Class C7(Of T) ' Noncompliant
        Inherits CA(Of CA(Of CA(Of CA(Of CA(Of CA(Of CA(Of CA(Of CA(Of CA(Of C7(Of CB(Of T))))))))))))
    End Class

    Class C8(Of T) ' Noncompliant
        Implements IComparable(Of C8(Of IEquatable(Of T)))
        Public Function CompareTo(other As C8(Of IEquatable(Of T))) As Integer Implements IComparable(Of C8(Of IEquatable(Of T))).CompareTo
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace

Namespace Tests.InterfaceRecursion

    ' some base classes
    Interface IA(Of T)
    End Interface

    Interface IB(Of T)
    End Interface

    Interface I0(Of T) ' Compliant
        Inherits IA(Of I0(Of T))
    End Interface

    Interface I1(Of T) ' Compliant
        Inherits IA(Of IB(Of I1 (Of T)))
    End Interface

    Interface I2(Of T) ' Compliant
        Inherits I0(Of T), IA(Of IB(Of I2(Of T)))
    End Interface

    Interface I3(Of T) ' Noncompliant
        Inherits IA(Of I3(Of IB(Of T)))
    End Interface

    Interface I4(Of T) ' Noncompliant  {{Refactor this Interface so that the generic inheritance chain is not recursive.}}
        Inherits IA(Of I4(Of I4(Of T)))
    End Interface

    Interface I5(Of T) ' Noncompliant
        Inherits IA(Of I5(Of IA(Of T)))
    End Interface

    Interface I6(Of T) ' Noncompliant
        Inherits IA(Of IA(Of IA(Of IA(Of IA(Of IA(Of IA(Of IA(Of IA(Of IA(Of IA(Of IA(Of I6(Of IB(Of T))))))))))))))
    End Interface
End Namespace
