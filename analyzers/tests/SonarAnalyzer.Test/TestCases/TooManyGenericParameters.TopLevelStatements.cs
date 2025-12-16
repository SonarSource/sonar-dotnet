void LocalFoo<T1, T2, T3>() { }
void LocalBar<T1, T2, T3, T4>() { } // Noncompliant {{Reduce the number of generic parameters in the 'LocalBar' method to no more than the 3 authorized.}}
