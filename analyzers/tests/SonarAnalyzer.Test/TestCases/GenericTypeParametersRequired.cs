using System;
using System.Collections.Generic;

namespace MyLibrary
{
    public class GenericClass<V>
    {
        public V SomeMethod() { return default(V); }
    }

    public class MyClass
    { }

    public class Foo
    {
        public void MyMethod<T>()  // Noncompliant {{Refactor this method to use all type parameters in the parameter list to enable type inference.}}
        //          ^^^^^^^^
        {
        }

        public void MyMethod_02<MyClass>(MyClass foo) { }
        public void MyMethod_03<T>(T foo, T foo2) { }
        public void MyMethod_04<TValue, TKey>(TKey foo, TValue foo2) { }

        public void MyMethod_05<TValue, TKey>(TKey foo) { } // Noncompliant
        public void MyMethod_06<TValue, TKey>(TValue foo) { } // Noncompliant

        public void MyMethod_09() { }
        public void MyMethod_10(int i) { }
        public void MyMethod_11(T i) { } // Error [CS0246]

        public void MyMethod_12<T>(IEquatable<T> foo) { }
        public void MyMethod_13<T, K>(Dictionary<K, T> foo) { }

        public void MyMethod_14<T, V>(Tuple<List<List<V>>, Tuple<ISet<T>, T>> foo) { }

        public void MyMethod_15<V>(params V[] p) { }
        // Error@+1 [CS0246]
        public void MyMethod_16<V>(params T[] p) { } // Noncompliant

        public void MyMethod_17<V>(V[] p) { }
        public void MyMethod_18<V>(MyClass[] p) { } // Noncompliant

        public void MyMethod_19<V>(List<V[]> p) { }
        public void MyMethod_20<V>(List<MyClass[]> p) { } // Noncompliant
        public void MyMethod_21<V>(List<V>[] p) { }
        // Error@+1 [CS0246]
        public void MyMethod_22<V>(List<T>[] p) { } // Noncompliant

        // See https://github.com/SonarSource/sonar-dotnet/issues/4548
        public TKey MyMethod_08<TKey>() { return default(TKey); } // Noncompliant FP

        public static T MyMethod_23<T>(string value) // Noncompliant FP
        {
            return (T) Convert.ChangeType(value, typeof(T));
        }
    }
}
