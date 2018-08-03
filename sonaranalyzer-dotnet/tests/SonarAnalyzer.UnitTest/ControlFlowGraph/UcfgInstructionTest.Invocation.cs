/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    public partial class UcfgInstructionTest
    {
        [TestMethod]
        public void TestSimpleMethodInvocation()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {

        public string Bar1() { return null; }
        public void Bar2() { /* no-op */ }

        public void Foo()
        {
            var variable = Bar1();
                // %0 := Namespace.Class1.Bar1() [ this ]
                // variable := __id [ %0 ]

            Bar1();
                // %1 := Namespace.Class1.Bar1() [ this ]

            Bar2();
                // %2 := Namespace.Class1.Bar2() [ this ]
        }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_MethodCallOnFieldAccess()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        private string Field;

        public void Foo()
        {
            var x = Field.ToLower();
                // %0 := __id [ this.Field ]
                // %1 := string.ToLower() [ %0 ]
                // x := __id [ %1 ]
        }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_LambdaArguments_Generate_Const()
        {
            const string code = @"
using System;
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s)
        {
            var x = Bar(s, a => a);
                // %0 := Namespace.Class1.Bar(string, System.Func<string, string>) [ this s const ]
                // x := __id [ %0 ]

            this.Bar(s, a => a);
                // %1 := Namespace.Class1.Bar(string, System.Func<string, string>) [ this s const ]
        }
        public string Bar(string s, Func<string, string> a)
        {
            return s;
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocation_MethodNameArguments_Generate_Const()
        {
            const string code = @"
namespace Ns1
{
    using System.Linq;

    public class Class1
    {
        public void Foo(string[] args)
        {
            var result = args.Select(long.Parse);
                // %0 := System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>) [ System.Linq.Enumerable args const ]
                // result := __id [ %0 ]

            var result = args.Select(LocalParse);
                // %1 := System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>) [ System.Linq.Enumerable args const ]
                // result := __id [ %1 ]

            var result = args.Select(this.LocalParse);
                // %2 := System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>) [ System.Linq.Enumerable args const ]
                // result := __id [ %2 ]
        }

        private long LocalParse(string s) => 1l;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocation_ArgumentIsMethodResult()
        {
            const string code = @"
namespace Ns1
{
    public class Class1
    {
        public void Foo()
        {
            Add(GetData());
                // %0 := Ns1.Class1.GetData() [ this ]
                // %1 := Ns1.Class1.Add(int) [ this %0 ]
        }

        public int GetData() => 1;
        public void Add(int value) { }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_ExtensionMethods()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        private string field;
        public void Foo(string s)
        {
            string a;
            a = s.Ext();
                // %0 := Namespace.Extensions.Ext(string) [ Namespace.Extensions s ]
                // a := __id [ %0 ]

            a = Extensions.Ext(s);
                // %1 := Namespace.Extensions.Ext(string) [ Namespace.Extensions s ]
                // a := __id [ %1 ]

            a = this.field.Ext();
                // %2 := __id [ this.field ]
                // %3 := Namespace.Extensions.Ext(string) [ Namespace.Extensions %2 ]
                // a := __id [ %3 ]
        }
    }
    public static class Extensions
    {
        public static string Ext(this string s) { return s; }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_Explicit_Interfaces()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foobar(string s, IBar b1, Bar b2)
        {
            b1.Foo(s);
                // %0 := Namespace.IBar.Foo(string) [ b1 s ]
            b2.Foo(s);
                // %1 := Namespace.Bar.Foo(string) [ b2 s ]
        }
    }
    public class Bar : IBar
    {
        void IBar.Foo(string s) { }
        public void Foo(string s) { }
    }
    public interface IBar
    {
        void Foo(string s);
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foobar");
        }

        [TestMethod]
        public void Invocations_Explicit_Generic_Interfaces()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foobar(string s, IBar b1, Bar b2, IBar<string> b3)
        {
            b1.Foo(s);
                // %0 := Namespace.IBar.Foo<T>(T) [ b1 s ]
            b2.Foo(s);
                // %1 := Namespace.Bar.Foo<T>(T) [ b2 s ]
            b2.Fooooo(s);
                // %2 := Namespace.Bar.Fooooo(string) [ b2 s ]
            b3.Fooooo(s);
                // %3 := Namespace.IBar<T>.Fooooo(T) [ b3 s ]
        }
    }
    public class Bar : IBar, IBar<string>
    {
        void IBar.Foo<T>(T s) { }
        public void Foo<T>(T s) { }
        public void Fooooo(string s) { }
        void IBar<string>.Fooooo(string s) { }
    }
    public interface IBar
    {
        void Foo<T>(T s);
    }
    public interface IBar<T>
    {
        void Fooooo(T s);
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foobar");
        }

        [TestMethod]
        public void Invocations_MethodsWithDefaultParameters()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            SendEmailAsync(""body"");
                // %0 := Namespace.Class1.SendEmailAsync(string, bool) [ this const const ]
            SendEmailAsync(""body"", isHtml: true);
                // %1 := Namespace.Class1.SendEmailAsync(string, bool) [ this const const ]
            SendEmailAsync(""body"", isHtml: false);
                // %2 := Namespace.Class1.SendEmailAsync(string, bool) [ this const const ]
        }

        public System.Threading.Tasks.Task SendEmailAsync(string body, bool isHtml = false)
        { return null; }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_UsedNamedParameters()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s)
        {
            SendEmailAsync(s, false);
                // %0 := Namespace.Class1.SendEmailAsync(string, bool) [ this s const ]
            SendEmailAsync(body: s, isHtml: false);
                // %1 := Namespace.Class1.SendEmailAsync(string, bool) [ this s const ]
            SendEmailAsync(body: s);
                // %2 := Namespace.Class1.SendEmailAsync(string, bool) [ this s const ]
            SendEmailAsync(isHtml: false, body: s);
                // %3 := Namespace.Class1.SendEmailAsync(string, bool) [ this s const ]
        }

        public System.Threading.Tasks.Task SendEmailAsync(string body, bool isHtml = false)
        { return null; }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_MethodParametersAreThisClassOrVariables()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        private int Field = 1;
        private static int StaticField = 2;
        public int Property { get; } = 3;
        public int StaticProperty { get; } = 4;
        private int InstanceMethod() => 5;
        private static int StaticMethod() => 6;

        public void Foo()
        {
            Bar(InstanceMethod());
                // %0 := Namespace.Class1.InstanceMethod() [ this ]
                // %1 := Namespace.Class1.Bar(int) [ this %0 ]

            Bar(StaticMethod());
                // %2 := Namespace.Class1.StaticMethod() [ Namespace.Class1 ]
                // %3 := Namespace.Class1.Bar(int) [ this %2 ]

            Bar(this.Property);
                // %4 := Namespace.Class1.Property.get [ this ]
                // %5 := Namespace.Class1.Bar(int) [ this %4 ]

            Bar(StaticProperty);
                // %6 := Namespace.Class1.StaticProperty.get [ this ]
                // %7 := Namespace.Class1.Bar(int) [ this %6 ]

            Bar(this.Field);
                // %8 := __id [ this.Field ]
                // %9 := Namespace.Class1.Bar(int) [ this %8 ]

            Bar(StaticField);
                // %10 := __id [ Namespace.Class1.StaticField ]
                // %11 := Namespace.Class1.Bar(int) [ this %10 ]

            Bar(Field, Property, this.InstanceMethod(), StaticMethod());
                // %12 := __id [ this.Field ]
                // %13 := Namespace.Class1.Property.get [ this ]
                // %14 := Namespace.Class1.InstanceMethod() [ this ]
                // %15 := Namespace.Class1.StaticMethod() [ Namespace.Class1 ]
                // %16 := Namespace.Class1.Bar(int, int, int, int) [ this %12 %13 %14 %15 ]
        }

        private void Bar(int arg1) { /* no-op */ }
        private void Bar(int arg1, int arg2, int arg3, int arg4) { /* no-op */ }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_QualifiedNames_Methods()
        {
            const string code = @"
public static class GlobalClass
{
    public static void NoOp() { }
}

namespace Ns1
{
    using static GlobalClass;

    public static class OuterClass
    {
        public static void NoOp() { }
    }

    namespace Inner
    {
        public static class InnerClass
        {
            public static void NoOp() { }

            public static class NestedClass
            {
                public static void NoOp() { }
            }
        }
    }

    public class Class1
    {
        public void Foo()
        {
            GlobalClass.NoOp();
                // %0 := GlobalClass.NoOp() [ GlobalClass ]

            Ns1.OuterClass.NoOp();
                // %1 := Ns1.OuterClass.NoOp() [ Ns1.OuterClass ]

            Ns1.Inner.InnerClass.NoOp();
                // %2 := Ns1.Inner.InnerClass.NoOp() [ Ns1.Inner.InnerClass ]

            Ns1.Inner.InnerClass.NestedClass.NoOp();
                // %3 := Ns1.Inner.InnerClass.NestedClass.NoOp() [ Ns1.Inner.InnerClass.NestedClass ]

            NoOp();
                // %4 := GlobalClass.NoOp() [ GlobalClass ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_QualifiedNames_PropertyGets()
        {
            const string code = @"
public static class GlobalClass
{
    public static int Property { get; } = 42;
}

namespace Ns1
{
    using static GlobalClass;

    public static class OuterClass
    {
        public static int Property { get; } = 42;
    }

    namespace Inner
    {
        public static class InnerClass
        {
            public static int Property { get; } = 42;

            public static class NestedClass
            {
                public static int Property { get; } = 42;
            }
        }
    }

    public class Class1
    {
        public void Foo()
        {
            int i;
            i = GlobalClass.Property;
                // %0 := GlobalClass.Property.get [ GlobalClass ]
                // i := __id [ %0 ]

            i = Ns1.OuterClass.Property;
                // %1 := Ns1.OuterClass.Property.get [ Ns1.OuterClass ]
                // i := __id [ %1 ]

            i = Ns1.Inner.InnerClass.Property;
                // %2 := Ns1.Inner.InnerClass.Property.get [ Ns1.Inner.InnerClass ]
                // i := __id [ %2 ]

            i = Ns1.Inner.InnerClass.NestedClass.Property;
                // %3 := Ns1.Inner.InnerClass.NestedClass.Property.get [ Ns1.Inner.InnerClass.NestedClass ]
                // i := __id [ %3 ]

            i = Property;
                // %4 := GlobalClass.Property.get [ GlobalClass ]
                // i := __id [ %4 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Invocations_QualifiedNames_PropertySets()
        {
            const string code = @"

public static class GlobalClass
{
    public static int Property { get; set; }
}

namespace Ns1
{
    using static GlobalClass;

    public static class OuterClass
    {
        public static int Property { get; set; }
    }

    namespace Inner
    {
        public static class InnerClass
        {
            public static int Property { get; set; }
            public static class NestedClass
            {
                public static int Property { get; set; }
            }
        }
    }

    public class Class1
    {
        public void Foo()
        {
            GlobalClass.Property = 42;
                // %0 := GlobalClass.Property.set [ GlobalClass const ]

            Ns1.OuterClass.Property = 42;
                // %1 := Ns1.OuterClass.Property.set [ Ns1.OuterClass const ]

            Ns1.Inner.InnerClass.Property = 42;
                // %2 := Ns1.Inner.InnerClass.Property.set [ Ns1.Inner.InnerClass const ]


            Ns1.Inner.InnerClass.NestedClass.Property = 42;
                // %3 := Ns1.Inner.InnerClass.NestedClass.Property.set [ Ns1.Inner.InnerClass.NestedClass const ]

            Property = 42;
                // %4 := GlobalClass.Property.set [ GlobalClass const ]
        }
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }
    }
}
