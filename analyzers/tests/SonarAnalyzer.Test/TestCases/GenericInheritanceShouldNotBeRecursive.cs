using System;

namespace Tests.ClassRecursion
{
    // some base classes
    class CA<T> { }
    class CB<T> { }
    class CC<T1, T2> { }

    class C0<T> : CA<C0<T>> { } // Compliant

    class C1<T> : CA<CB<C1<T>>> { } // Compliant

    class C2<T> : CA<C2<CB<T>>> { } // Noncompliant {{Refactor this class so that the generic inheritance chain is not recursive.}}
//        ^^

    class C3<T> : CA<C3<C3<T>>> { } // Noncompliant

    class C4<T> : CA<C4<CA<T>>> { } // Noncompliant

    class C5<T> : CC<C5<CA<T>>, CB<T>> { } // Noncompliant

    class C6<T> : CC<C5<CA<T>>, CB<T>> { } // Compliant

    class C7<T> : CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<C7<CB<T>>>>>>>>>>>>>> { } // Noncompliant

    // Error@+1 [CS0535]
    class C8<T> : IComparable<C8<IEquatable<T>>> { } // Noncompliant
}

namespace Tests.InterfaceRecursion
{
    // some base classes
    interface IA<T> { }
    interface IB<T> { }

    interface I0<T> : IA<I0<T>> { } // Compliant

    interface I1<T> : IA<IB<I1<T>>> { } // Compliant

    interface I2<T> : I0<int>, IA<IB<I2<T>>> { } // Compliant

    interface I3<T> : IA<I3<IB<T>>> { } // Noncompliant  {{Refactor this interface so that the generic inheritance chain is not recursive.}}

    interface I4<T> : IA<I4<I4<T>>> { } // Noncompliant

    interface I5<T> : IA<I5<IA<T>>> { } // Noncompliant

    interface I6<T> : IA<IA<IA<IA<IA<IA<IA<IA<IA<IA<IA<IA<I6<IB<T>>>>>>>>>>>>>> { } // Noncompliant
}
