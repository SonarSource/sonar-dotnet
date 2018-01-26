using System;

namespace Tests.SingleClassRecursion
{
    // some base classes
    class CA<T> { }
    class CB<T> { }
    class CC<T1, T2> { }

    class C0<T> : CA<C0<T>> { } // Compliant

    class C1<T> : CA<CB<C1<T>>> { } // Compliant

    class C2<T> : CA<C2<CB<T>>> { } // Noncompliant

    class C3<T> : CA<C3<C3<T>>> { } // Noncompliant

    class C4<T> : CA<C4<CA<T>>> { } // Noncompliant

    class C5<T> : CC<C5<CA<T>>, CB<T>> { } // Noncompliant

    class C6<T> : CC<C5<CA<T>>, CB<T>> { } // Compliant

    class C7<T> : CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<C7<CB<T>>>>>>>>>>>>>> { } // Noncompliant
}
