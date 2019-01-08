/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class AvoidExcessiveClassCouplingTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling()
        {
            Verifier.VerifyAnalyzer(@"TestCases\AvoidExcessiveClassCoupling.cs",
                new AvoidExcessiveClassCoupling { Threshold = 1 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Generic_No_Constraints()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Generics1 // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 1 to the maximum authorized 0 or less.}}
{
    public void Foo<T, V>(IDictionary<T, V> dictionary) { } // +1 for dictionary
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Generic_With_Constraints()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Generics1 // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 4 to the maximum authorized 0 or less.}}
{
    public void Foo1<T, V>(IDictionary<T, V> dictionary)  // +1 for IDictionary
        where T : IEnumerable<IDisposable> // +1 for IEnumerable, +1 for IDisposable
        where V : ICloneable // +1 for ICloneable
    {
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Generic_Bounded()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Generics1 // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 3 to the maximum authorized 0 or less.}}
{
    public void Foo(IDictionary<IDisposable, ICloneable> dictionary)  // +1 for IDictionary, +1 for IDisposable, +1 for ICloneable
    {
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Generic_Bounded_Deep_Nesting()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Generics1 // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 6 to the maximum authorized 0 or less.}}
{
    public void Foo(IList<ICollection<IEnumerable<IComparer<Stack<Queue<int>>>>>> dictionary)
    // +1 for IList, +1 for ICollection, +1 for IEnumerable, +1 for IComparer, +1 for Stack, +1 for Queue
    {
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Task_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System.Threading.Tasks;
public class Tasks // Compliant, Task types are not counted
{
    public void Foo<T>(Task task1, Task<T> task2) { }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Action_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class Actions // Compliant, Action types are not counted
{
    public void Foo<T>(Action action1, Action<T> action2, Action<T, T> action3, Action<T, T, T> action4, Action<T, T, T, T> action5) { }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Func_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class Functions // Compliant, Func types are not counted
{
    public void Foo<T>(Func<T> func1, Func<T, T> func2, Func<T, T, T> func3, Func<T, T, T, T> func4) { }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Pointers_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class Pointers // Compliant, pointers are not counted
{
    public void Foo(int* pointer) { } // Error [CS0214]
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Enums_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class Pointers // Compliant, enums are not counted
{
    public ConsoleColor Foo(ConsoleColor c) { return ConsoleColor.Black; }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Lazy_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Lazyness // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 1 to the maximum authorized 0 or less.}}
{
    public void Foo(Lazy<IEnumerable<int>> lazy) { } // +1 IEnumerable
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Primitive_Types_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
public class Types // Compliant, pointers are not counted
{
    public void Foo(bool b1,
        byte b2, sbyte b3, int i1, uint i2, long i3, ulong i4,
        IntPtr p1, UIntPtr p2,
        char c1, float d1,
        double d2, string s1,
        object o1) { }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Fields_Are_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System.Collections.Generic;
public class Fields // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 5 to the maximum authorized 0 or less.}}
{
    // accessibility
    private IList<int> c1;
    public ICollection<int> c2;
    internal IEnumerable<int> c3;
    protected IEnumerator<int> c4;
    // static
    public static IEqualityComparer<int> c5;
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Properties_Are_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System.Collections.Generic;
public class Properties // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 9 to the maximum authorized 0 or less.}}
{
    // accessibility
    private IList<int> C1 { get; set; }
    public ICollection<int> C2 { get; set; }
    internal IEnumerable<int> C3 { get; set; }
    protected IEnumerator<int> C4 { get; set; }
    // static
    public static IEqualityComparer<int> C5 { get; set; }
    // accessor bodies
    public Stack<int> C6
    {
        get
        {
            Queue<int> q;
            return null;
        }
        set
        {
            List<int> l;
        }
    }
    // expression body
    public object C7 => new Dictionary<int, int>();
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Indexers_Are_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System.Collections.Generic;
public class Indexers // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 6 to the maximum authorized 0 or less.}}
{
    // accessibility
    public IList<int> this[int i] { get { return null; } } // +1 IList
    private ICollection<int> this[int i, int j] { get { return null; } } // +1 ICollection
    protected IEnumerable<int> this[int i, int j, int k] { get { return null; } } // +1 IEnumerable
    internal IEnumerator<int> this[int i, int j, int k, int l] { get { return null; } } // +1 IEnumerator
    // parameters
    public int this[IEqualityComparer<int> i, Stack<int> j] { get { return 0; } } // +1 IEqualityComparer, +1 Stack
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Events_Are_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Events // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 9 to the maximum authorized 0 or less.}}
{
    // accessibility
    private event EventHandler<IList<int>> e1; // +1 EventHandler, +1 IList
    public event EventHandler<ICollection<int>> e2; // +1 ICollection
    internal event EventHandler<IEnumerable<int>> e3; // +1 IEnumerable
    protected event EventHandler<IEnumerator<int>> e4; // +1 IEnumerator
    // static
    public static event EventHandler<IEqualityComparer<int>> e5; // +1 IEqualityComparer
    // accessor bodies
    public event EventHandler<Stack<int>> E6 // +1 Stack
    {
        add
        {
            Queue<int> q; // +1 Queue
        }
        remove
        {
            List<int> l; // +1 List
        }
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Methods_Are_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
using System.Diagnostics;
public class Methods // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 10 to the maximum authorized 0 or less.}}
{
    // accessibility
    private void M1(IList<int> l1) { } // +1 IList
    public void M2(ICollection<int> l1) { } // +1 ICollection
    internal void M3(IEnumerable<int> l1) { } // +1 IEnumerable
    protected void M4(IEnumerator<int> l1) { } // +1 IEnumerator
    // return type
    private IEqualityComparer<int> M5() { return null; } // +1 IEqualityComparer
    // generic constraints
    private void M6<T>() where T : Stack<int> { return; } // +1 Stack
    // method body
    private void M8()
    {
        Queue<int> q; // +1 Queue
        Console.Clear(); // +1 Console
}
    // expression body
    private object M9() => new List<int>(); // +1 List
    private void M10() => Debug.Write(1); // +1 Debug
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Inner_Classes_And_Structs_Are_Not_Counted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class OuterClass // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 1 to the maximum authorized 0 or less.}}
{
    private void M1(IList<int> l1) { } // +1 IList

    public class InnerClass // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 1 to the maximum authorized 0 or less.}}
    {
        private void M1(ICollection<int> l1) { } // +1 ICollection
    }
}
public struct OuterStruct // Noncompliant {{Split this struct into smaller and more specialized ones to reduce its dependencies on other classes from 1 to the maximum authorized 0 or less.}}
{
    private void M1(IList<int> l1) { } // +1 IList

    public struct InnerStruct // Noncompliant {{Split this struct into smaller and more specialized ones to reduce its dependencies on other classes from 1 to the maximum authorized 0 or less.}}
    {
        private void M1(ICollection<int> l1) { } // +1 ICollection
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Interface_Declaration()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public interface I // Noncompliant {{Split this interface into smaller and more specialized ones to reduce its dependencies on other classes from 1 to the maximum authorized 0 or less.}}
{
    void M1(IList<int> l1); // +1 IList
    // interfaces cannot contain other types
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Self_Reference()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Self // Compliant, self references are not counted
{
    void M1(Self other) { }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Base_Classes_Interfaces_NotCounted()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Base {}
public class Self // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 2 to the maximum authorized 0 or less.}}
    : Base, ICloneable
{
    public object Clone() { return null; }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Catch_Statements()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using System.Collections.Generic;
public class Self // Noncompliant {{Split this class into smaller and more specialized ones to reduce its dependencies on other classes from 2 to the maximum authorized 0 or less.}}
{
    void M1()
    {
        try { } catch (Exception) { } // +1 Exception
        try { } catch (Exception e) when (e is InvalidOperationException) { } // +1 InvalidOperationException
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Attributes()
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
[Serializable]
public class Self // Compliant, attributes are not counted
{
    [Obsolete]
    void M1()
    {
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void AvoidExcessiveClassCoupling_Nameof()
        {
            Verifier.VerifyCSharpAnalyzer(@"
public class A // Compliant, types referenced by the nameof expression are not counted
{
    public A()
    {
        var s1 = nameof(System.Type);
        var s2 = nameof(System.Action);
    }
}
",
                new AvoidExcessiveClassCoupling { Threshold = 0 });
        }
    }
}
