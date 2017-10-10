using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IMyInterface1
    {
        unsafe string Method(int[,] parameter, int* pointer);
    }
    public interface IMyInterface2
    {
        unsafe int Method(int[,] parameter, int* pointer);
//                 ^^^^^^ Secondary
    }
    public interface IMyInterfaceCommon1 : IMyInterface1, IMyInterface2 // Noncompliant {{Rename or add member 'Method(int[*,*], int*)' to this interface to resolve ambiguities.}}
//                   ^^^^^^^^^^^^^^^^^^^
    {
        int Method(int i, string s);
    }
    public interface IMyInterfaceCommon2 : IMyInterface1, IMyInterface2
    {
        unsafe new int Method(int[,] parameter, int* pointer);
    }

    public interface IMyInterfaceGeneric1
    {
        unsafe string Method<T1>(T1[,] parameter, int* pointer);
    }
    public interface IMyInterfaceGeneric2
    {
        unsafe int Method<T2>(T2[,] parameter, int* pointer);
    }
    public interface IMyInterfaceGenericCommon1 : IMyInterfaceGeneric1, IMyInterfaceGeneric2 // Non-compliant, but not recognized
    {
    }

    public interface IMyInterfaceGeneric3<T>
    {
        unsafe int Method(T[,] parameter, int* pointer);
//                 ^^^^^^ Secondary
//                 ^^^^^^ Secondary@-1
    }

    public interface IMyInterfaceGeneric4<T>
    {
        unsafe int Method(T[,] parameter, int* pointer);
    }

    public interface IMyInterfaceGenericCommon2 : IMyInterface1, IMyInterfaceGeneric3<int> // Noncompliant
    {
    }

    public interface IMyInterfaceGenericCommon3<T> : IMyInterfaceGeneric4<T>, IMyInterfaceGeneric3<T> // Noncompliant
    {
    }

    public interface IMyInterfaceGenericCommon4<T> : IMyInterfaceGeneric4<T>, IMyInterfaceGeneric3<T>
    {
        unsafe int Method(T[,] parameter, int* pointer);
    }

    public interface IMyInterfaceEvent1
    {
        event EventHandler X;
    }
    public interface IMyInterfaceEvent2
    {
        event EventHandler X;
//                         ^ Secondary
    }
    public interface IMyInterfaceCommonEvent : IMyInterfaceEvent1, IMyInterfaceEvent2 // Noncompliant
    {
    }


    public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>, //Noncompliant
        IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
    }
    public interface IMultilayer : IReadOnlyDictionary<string, object>, IDummy // Compliant
    {
    }
    public interface IDummy
    {
    }
}
