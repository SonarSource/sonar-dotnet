record CA<T> { }
record CB<T> { }
record CC<T1, T2> { }
record C0<T> : CA<C0<T>> { } // Compliant
record C1<T> : CA<CB<C1<T>>> { } // Compliant
record C2<T> : CA<C2<CB<T>>> { } // Noncompliant {{Refactor this record so that the generic inheritance chain is not recursive.}}
record C3<T> : CA<C3<C3<T>>> { } // Noncompliant
record C4<T> : CA<C4<CA<T>>> { } // Noncompliant
record C5<T> : CC<C5<CA<T>>, CB<T>> { } // Noncompliant
record C6<T> : CC<C5<CA<T>>, CB<T>> { } // Compliant
record C7<T> : CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<C7<CB<T>>>>>>>>>>>>>> { } // Noncompliant
record C8<T>(string parameter) : CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<CA<C8<CB<T>>>>>>>>>>>>>> { } // Noncompliant
