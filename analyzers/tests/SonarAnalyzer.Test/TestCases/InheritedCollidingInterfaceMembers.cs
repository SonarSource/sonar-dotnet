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
//                 ^^^^^^ Secondary {{This member collides with 'IMyInterface1.Method(int[*,*], int*)'}}
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
    public interface IMyInterfaceGenericCommon1 : IMyInterfaceGeneric1, IMyInterfaceGeneric2 // FN, no support for generics with different type name
    {
    }

    public interface IMyInterfaceGeneric3<T>
    {
        unsafe int Method(T[,] parameter, int* pointer);
//                 ^^^^^^ Secondary {{This member collides with 'IMyInterface1.Method(int[*,*], int*)'}}
//                 ^^^^^^ Secondary@-1 {{This member collides with 'IMyInterfaceGeneric4<T>.Method(T[*,*], int*)'}}
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
//                         ^ Secondary {{This member collides with 'IMyInterfaceEvent1.X'}}
    }
    public interface IMyInterfaceCommonEvent : IMyInterfaceEvent1, IMyInterfaceEvent2 // Noncompliant
    {
    }

    public interface IMultipleMember1
    {
        int Method(int i);
        void Method2(string s);
    }

    public interface IMultipleMember2
    {
        void Method2(string s);
//           ^^^^^^^ Secondary {{This member collides with 'IMultipleMember1.Method2(string)'}}
    }

    public interface IMultipleMember3
    {
        int Method(int i);
//          ^^^^^^ Secondary {{This member collides with 'IMultipleMember1.Method(int)'}}
    }

    public interface IMultipleMemberCommon : IMultipleMember1, IMultipleMember2, IMultipleMember3 // Noncompliant {{Rename or add members 'Method(int)' and 'Method2(string)' to this interface to resolve ambiguities.}}
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

    // https://github.com/SonarSource/sonar-dotnet/issues/3225
    public interface IRepro3225<T> : IList<T>, IReadOnlyList<T> // Noncompliant {{Rename or add member 'IReadOnlyList<T>.this[int]' to this interface to resolve ambiguities.}}
    {
    }
}
