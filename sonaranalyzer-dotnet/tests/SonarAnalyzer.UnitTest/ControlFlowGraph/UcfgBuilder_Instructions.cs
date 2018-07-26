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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    [TestClass]
    public class UcfgBuilder_Instructions
    {
        [TestMethod]
        public void Assignments_Simple()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public static string staticField;

        private string field;
        public void Foo(string s)
        {
            string a, b;

            a = s;
                // a := __id [ s ]

            a = ""boo"";
                // a := __id [ const ]

            int i;
            i = 5;
                // i := __id [ const ]

            field = s;
                // this.field := __id [ s ]
            this.field = s;
                // this.field := __id [ s ]

            staticField = s;
                // Namespace.Class1.staticField := __id [ s ]
            Class1.staticField = s;
                // Namespace.Class1.staticField := __id [ s ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Nested()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s)
        {
            string a, b, c;
            a = b = c = ""foo""
                // c := __id [ const ]
                // b := __id [ c ]
                // a := __id [ b ]

            int x, y, z;
            x = y = z = 5;
                // z := __id [ const ]
                // y := __id [ z ]
                // x := __id [ y ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Static_Property_On_Generic_Class()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            var x = Bar<string>.Value;
                // %0 := Namespace.Bar<T>.Value.get [ Namespace.Bar<T> ]
                // x := __id [ %0 ]

            var y = Bar<int>.Value;
                // %1 := Namespace.Bar<T>.Value.get [ Namespace.Bar<T> ]
                // y := __id [ %1 ]
        }
    }
    public class Bar<T>
    {
        public static T Value { get { return default(T); } }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Properties()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public static string StaticProperty { get; set; }

        public string Property { get; set; }
        public int IntProperty { get; set; }
        public object ObjectProperty { get; set; }
        public string Foo(string s)
        {
            string a;
            Property = s;
                // %0 := Namespace.Class1.Property.set [ this s ]

            a = Property;
                // %1 := Namespace.Class1.Property.get [ this ]
                // a := __id [ %1 ]

            Property = Property;
                // %2 := Namespace.Class1.Property.get [ this ]
                // %3 := Namespace.Class1.Property.set [ this %2 ]

            Foo(Property);
                // %4 := Namespace.Class1.Property.get [ this ]
                // %5 := Namespace.Class1.Foo(string) [ this %4 ]

            Property = Foo(Property);
                // %6 := Namespace.Class1.Property.get [ this ]
                // %7 := Namespace.Class1.Foo(string) [ this %6 ]
                // %8 := Namespace.Class1.Property.set [ this %7 ]

            ObjectProperty = s;
                // %9 := Namespace.Class1.ObjectProperty.set [ this s ]

            ObjectProperty = 5;
                // %10 := Namespace.Class1.ObjectProperty.set [ this const ]

            var x = IntProperty = 5;
                // %11 := Namespace.Class1.IntProperty.set [ this const ]
                // x := __id [ %11 ]

            this.Property = s;
                // %12 := Namespace.Class1.Property.set [ this s ]

            var other = new Class1();
                // %13 := new Namespace.Class1
                // %14 := Namespace.Class1.Class1() [ %13 ]
                // other := __id [ %13 ]

            other.Property = s;
                // %15 := Namespace.Class1.Property.set [ other s ]

            other.ObjectProperty = other.Property;
                // %16 := Namespace.Class1.Property.get [ other ]
                // %17 := Namespace.Class1.ObjectProperty.set [ other %16 ]

            Class1.StaticProperty = s;
                // %18 := Namespace.Class1.StaticProperty.set [ Namespace.Class1 s ]

            StaticProperty = s;
                // %19 := Namespace.Class1.StaticProperty.set [ Namespace.Class1 s ]

            a = StaticProperty;
                // %20 := Namespace.Class1.StaticProperty.get [ Namespace.Class1 ]
                // a := __id [ %20 ]

            a = Class1.StaticProperty;
                // %21 := Namespace.Class1.StaticProperty.get [ Namespace.Class1 ]
                // a := __id [ %21 ]

            return s;
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_AutoProperties()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        private string Property { get; }
        public Class1(string s)
        {
            Property = s;
                // ignored, property has no SetMethod
                // This should be ok for now because we don't really care about
                // properties without accessors, because they behave as pure fields.
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Class1", isCtor: true);
        }

        [TestMethod]
        public void Assignments_Concatenations()

        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        private string field;
        public void Foo(string s, string t, object o)
        {
            string a = s;
                // a := __id [ s ]

            a = s + s;
                // %0 := __concat [ s s ]
                // a := __id [ %0 ]

            a = s + s + s;
                // %1 := __concat [ s s ]
                // %2 := __concat [ %1 s ]
                // a := __id [ %2 ]

            a = (s + s) + t;
                // %3 := __concat [ s s ]
                // %4 := __concat [ %3 t ]
                // a := __id [ %4 ]

            a = s + 5;
                // %5 := __concat [ s const ]
                // a := __id [ %5 ]

            a = 5 + s;
                // %6 := __concat [ const s ]
                // a := __id [ %6 ]

            a = s + o;
                // %7 := __concat [ s o ]
                // a := __id [ %7 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Invocations()
        {
            const string code = @"
namespace Namespace
{
    public class BaseClass
    {
        public void BaseMethod() {}
    }

    public class Class1 : BaseClass
    {
        public string field;
        public void Foo(string s)
        {
            string a;
            a = s.ToLower();
                // %0 := string.ToLower() [ s ]
                // a := __id [ %0 ]

            a = (s + s).ToLower();
                // %1 := __concat [ s s ]
                // %2 := string.ToLower() [ %1 ]
                // a := __id [ %2 ]

            Bar(s.ToLower());
                // %3 := string.ToLower() [ s ]
                // %4 := Namespace.Class1.Bar(string) [ this %3 ]

            a = string.IsNullOrEmpty(s);
                // %5 := string.IsNullOrEmpty(string) [ string s ];
                // a := __id [ %5 ]

            a = A(B(C(s)));
                // %6 := Namespace.Class1.C(string) [ this s ]
                // %7 := Namespace.Class1.B(int) [ this %6 ]
                // %8 := Namespace.Class1.A(int) [ this %7 ]
                // a := __id [ %8 ]

            int x;
            x = O(s);
                // %9 := Namespace.Class1.O(object) [ this s ]
                // x := __id [ %9 ]

            this.Bar(s);
                // %10 := Namespace.Class1.Bar(string) [ this s ]

            base.BaseMethod();
                // %11 := Namespace.BaseClass.BaseMethod() [ this ]

            var other = new Class1();
                // %12 := new Namespace.Class1
                // %13 := Namespace.Class1.Class1() [ %12 ]
                // other := __id [ %12 ]

            other.Bar(s);
                // %14 := Namespace.Class1.Bar(string) [ other s ]

            Bar(field);
                // %15 := __id [ this.field ]
                // %16 := Namespace.Class1.Bar(string) [ this %15 ]

            Bar(other.field);
                // %17 := __id [ other.field ]
                // %18 := Namespace.Class1.Bar(string) [ this %17 ]

            StaticMethod(s);
                // %19 := Namespace.Class1.StaticMethod(string) [ Namespace.Class1 s ]

            Class1.StaticMethod(s);
                // %20 := Namespace.Class1.StaticMethod(string) [ Namespace.Class1 s ]

            a = field;
                // %21 := __id [ this.field ]
                // a := __id [ %21 ]
        }
        public void Bar(string s) { }
        public string A(int x) { return x.ToString(); }
        public int B(int x) { return x; }
        public int C(string s) { 5; }
        public int O(object o) { 5; }

        public static string StaticMethod(string s) { return s; }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Ctors()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public static Class1 staticField;
        public Class1 field;

        public string Property { get; set; }
        public Class1() { }
        public Class1(string s) { }
        public Class1(Class1 other) { }
        public void Foo(string s)
        {
            Class1 c;
            c = new Class1(s);
                // %0 := new Namespace.Class1
                // %1 := Namespace.Class1.Class1(string) [ %0 s ]
                // c := __id [ %0 ]

            c = new Class1();
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // c := __id [ %2 ]

            c = new Class1(new Class1(s));
                // %4 := new Namespace.Class1
                // %5 := Namespace.Class1.Class1(string) [ %4 s ]
                // %6 := new Namespace.Class1
                // %7 := Namespace.Class1.Class1(Namespace.Class1) [ %6 %4 ]
                // c := __id [ %6 ]

            c = new Class1(s)
                // %8 := new Namespace.Class1
                // %9 := Namespace.Class1.Class1(string) [ %8 s ]
            {
                Property = s,
                    // %10 := Namespace.Class1.Property.set [ %8 s ]
            };
                // c := __id [ %8 ]

            field = new Class1();
                // %11 := new Namespace.Class1
                // %12 := Namespace.Class1.Class1() [ %11 ]
                // this.field := __id [ %11 ]

            this.field = new Class1();
                // %13 := new Namespace.Class1
                // %14 := Namespace.Class1.Class1() [ %13 ]
                // this.field := __id [ %13 ]

            staticField = new Class1();
                // %15 := new Namespace.Class1
                // %16 := Namespace.Class1.Class1() [ %15 ]
                // Namespace.Class1.staticField := __id [ %15 ]

            Class1.staticField = new Class1();
                // %17 := new Namespace.Class1
                // %18 := Namespace.Class1.Class1() [ %17 ]
                // Namespace.Class1.staticField := __id [ %17 ]

            var other = new Class1();
                // %19 := new Namespace.Class1
                // %20 := Namespace.Class1.Class1() [ %19 ]
                // other := __id [ %19 ]

            other.field = this;
                // other.field := __id [ this ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Static_Ctors()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        static Class1()
        {
            Do();
                // %0 := Namespace.Class1.Do() [ Namespace.Class1 ]
        }

        public static void Do() {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Class1", true);
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

        [TestMethod]
        public void TestSimpleAssignment()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string arg)
        {
            var variable = arg;
                // variable := __id [ arg ]
        }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void TestSimpleFieldAccessAndAssignment()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string Field;

        public void Foo()
        {
            var variable = Field;
                // %0 := __id [ this.Field ]
                // variable := __id [ %0 ]

        }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

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
        public void ControllerMethod_Contains_EntryPoint_And_Attributes()
        {
            const string code = @"
using System.ComponentModel;
using System.Web.Mvc;
public class Class1 : Controller
{
    public class DummyAttribute : System.Attribute { }

    private string field;
    [HttpPost] // should be ignored
    public void Foo([Description]string s, [Missing]string x,
                    [Dummy] int i, [DummyAttribute]string s2) {}
        // %0 := __entrypoint [ s x const s2 ]
        // %1 := __annotate [ System.ComponentModel.DescriptionAttribute.DescriptionAttribute() s ]
        // s := __annotation [ %1 ]
        // i is a const so the attribute is ignored
        // the Missing attribute is unknown and is not included
        // %2 := __annotate [ Class1.DummyAttribute.DummyAttribute() s2 ]
        // s2 := __annotation [ %2 ]
}";
            var ucfg = UcfgVerifier.VerifyInstructions(code, "Foo");

            var entryPoints = UcfgVerifier.GetEntryPointInstructions(ucfg);
            entryPoints.Count.Should().Be(1);

            // Entry point location should be the "Foo" token.
            // Line numbers are 1-based, offsets are 0-based
            var actualLocation = entryPoints[0].Assigncall.Location;
            actualLocation.StartLine.Should().Be(10);
            actualLocation.EndLine.Should().Be(10);
            actualLocation.StartLineOffset.Should().Be(16);
            actualLocation.EndLineOffset.Should().Be(18);
        }

        [TestMethod]
        public void Annotation_EntryMethod_AttributeOnStringParameterIsHandled()
        {
            const string code = @"
namespace Namespace
{
    using System.Web.Mvc;

    public class FromBodyAttribute : System.Attribute { }

    public class CartController : Controller
    {
        public object Remove([FromBody] string itemId)
        {
            var data = itemId;
                // data := __id [ itemId ]
                // %0 := __entrypoint [ itemId ]
                // %1 := __annotate [ Namespace.FromBodyAttribute.FromBodyAttribute() itemId ]
                // itemId := __annotation [ %1 ]

            return null;
        }
    }
}";
            var ucfg = UcfgVerifier.VerifyInstructions(code, "Remove");

            var entryPoints = UcfgVerifier.GetEntryPointInstructions(ucfg);
            entryPoints.Count.Should().Be(1);

            // Entry point location should be the "Remove" token.
            // Line numbers are 1-based, offsets are 0-based
            var actualLocation = entryPoints[0].Assigncall.Location;
            actualLocation.StartLine.Should().Be(10);
            actualLocation.EndLine.Should().Be(10);
            actualLocation.StartLineOffset.Should().Be(22);
            actualLocation.EndLineOffset.Should().Be(27);
        }

        [TestMethod]
        public void Annotation_EntryMethod_AttributeOnNonStringParameterIsHandled()
        {
            // Bug 169
            const string code = @"
namespace Namespace
{
    using System.Web.Mvc;

    public class FromBodyAttribute : System.Attribute { }

    public class CartController : Controller
    {
        public object Remove([FromBody] long itemId)
        {
            var data = itemId;
                // data := __id [ const ]
                // %0 := __entrypoint [ const ]

            return null;
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Remove");
        }

        [TestMethod]
        public void Annotation_NotEntryMethod_AttibutesAreIgnored()
        {
            const string code = @"
namespace Namespace
{
    public class FromBodyAttribute : System.Attribute { }

    public class NotAController
    {
        public object Foo([FromBody] long itemId, [FromBody] string itemId2)
        {
            var data = itemId;
                // data := __id [ const ]
            var data = itemId2;
                // data := __id [ itemId2 ]
            return null;
        }
    }
}";
            var ucfg = UcfgVerifier.VerifyInstructions(code, "Foo");

            var entryPoints = UcfgVerifier.GetEntryPointInstructions(ucfg);
            entryPoints.Count.Should().Be(0);
        }

        [TestMethod]
        public void ConstantExpressions_Share_The_Same_Instance()
        {
            const string code = @"
public class Class1
{
    private string field;
    public void Foo(string s)
    {
        string a = ""a"";
            // a := __id [ const ]
        string b = ""b"";
            // b := __id [ const ]
        string c = ""c"";
            // c := __id [ const ]
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            var a = ucfg.BasicBlocks[0].Instructions[0].Assigncall.Args[0];
            var b = ucfg.BasicBlocks[0].Instructions[1].Assigncall.Args[0];
            var c = ucfg.BasicBlocks[0].Instructions[2].Assigncall.Args[0];

            // The constant expressions share the same instance of the Const value
            // for performance and simplicity. The protobuf serializer will deserialize
            // the values as a singleton again.
            a.Should().Be(b);
            a.Should().Be(c);
        }

        [TestMethod]
        public void BuiltInTypeAreConvertedToConstant()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(bool boolArg, byte byteArg, sbyte sbyteArg, char charArg, decimal decimalArg, double doubleArg,
            float floatArg, int intArg, uint uintArg, long longArg, ulong ulongArg, short shortArg, ushort ushortArg)
        {
            bool @bool = true;      // @bool := __id [ const ]
            byte @byte = 1;         // @byte := __id [ const ]
            sbyte @sbyte = 1;       // @sbyte := __id [ const ]
            char @char = 'c';       // @char := __id [ const ]
            decimal @decimal = 1;   // @decimal := __id [ const ]
            double @double = 1;     // @double := __id [ const ]
            float @float = 1;       // @float := __id [ const ]
            int @int = 1;           // @int := __id [ const ]
            uint @uint = 1;         // @uint := __id [ const ]
            long @long = 1;         // @long := __id [ const ]
            ulong @ulong = 1;       // @ulong := __id [ const ]
            short @short = 1;       // @short := __id [ const ]
            ushort @ushort = 1;     // @ushort := __id [ const ]

            Bar(true);              // %0 := Namespace.Class1.Bar(object) [ this const ]
            Bar((byte) 1);          // %1 := Namespace.Class1.Bar(object) [ this const ]
            Bar((sbyte) 1);         // %2 := Namespace.Class1.Bar(object) [ this const ]
            Bar('c');               // %3 := Namespace.Class1.Bar(object) [ this const ]
            Bar((decimal) 1);       // %4 := Namespace.Class1.Bar(object) [ this const ]
            Bar((double) 1);        // %5 := Namespace.Class1.Bar(object) [ this const ]
            Bar((float) 1);         // %6 := Namespace.Class1.Bar(object) [ this const ]
            Bar((int) 1);           // %7 := Namespace.Class1.Bar(object) [ this const ]
            Bar((uint) 1);          // %8 := Namespace.Class1.Bar(object) [ this const ]
            Bar((long) 1);          // %9 := Namespace.Class1.Bar(object) [ this const ]
            Bar((ulong) 1);         // %10 := Namespace.Class1.Bar(object) [ this const ]
            Bar((short) 1);         // %11 := Namespace.Class1.Bar(object) [ this const ]
            Bar((ushort) 1);        // %12 := Namespace.Class1.Bar(object) [ this const ]

            Bar(boolArg);           // %13 := Namespace.Class1.Bar(object) [ this const ]
            Bar(byteArg);           // %14 := Namespace.Class1.Bar(object) [ this const ]
            Bar(sbyteArg);          // %15 := Namespace.Class1.Bar(object) [ this const ]
            Bar(charArg);           // %16 := Namespace.Class1.Bar(object) [ this const ]
            Bar(decimalArg);        // %17 := Namespace.Class1.Bar(object) [ this const ]
            Bar(doubleArg);         // %18 := Namespace.Class1.Bar(object) [ this const ]
            Bar(floatArg);          // %19 := Namespace.Class1.Bar(object) [ this const ]
            Bar(intArg);            // %20 := Namespace.Class1.Bar(object) [ this const ]
            Bar(uintArg);           // %21 := Namespace.Class1.Bar(object) [ this const ]
            Bar(longArg);           // %22 := Namespace.Class1.Bar(object) [ this const ]
            Bar(ulongArg);          // %23 := Namespace.Class1.Bar(object) [ this const ]
            Bar(shortArg);          // %24 := Namespace.Class1.Bar(object) [ this const ]
            Bar(ushortArg);         // %25 := Namespace.Class1.Bar(object) [ this const ]
        }

        public void Bar(object o) {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void FieldChaining()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public Class1 field = new Class1();
        public static Class1 staticField = new Class1();

        public void Foo()
        {
            field.field.field = new Class1();
                // %0 := __id [ this.field ]
                // %1 := __id [ %0.field ]
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // %1.field := __id [ %2 ]

            Class1.staticField.field = new Class1();
                // %4 := __id [ Namespace.Class1.staticField ]
                // %5 := new Namespace.Class1
                // %6 := Namespace.Class1.Class1() [ %5 ]
                // %4.field := __id [ %5 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void PropertyChaining()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public Class1 Property { get; set; }
        public static Class1 StaticProperty { get; set; }

        public void Foo()
        {
            Property.Property.Property = new Class1();
                // %0 := Namespace.Class1.Property.get [ this ]
                // %1 := Namespace.Class1.Property.get [ %0 ]
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // %4 := Namespace.Class1.Property.set [ %1 %2 ]

            Class1.StaticProperty.Property = new Class1();
                // %5 := Namespace.Class1.StaticProperty.get [ Namespace.Class1 ]
                // %6 := new Namespace.Class1
                // %7 := Namespace.Class1.Class1() [ %6 ]
                // %8 := Namespace.Class1.Property.set [ %5 %6 ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Pointers()
        {
            const string code = @"
using System;
namespace Namespace
{
    public class Class1
    {
        IntPtr ptrField;
        UIntPtr uPtrField;

        public void Foo(IntPtr ptrParam, UIntPtr uPtrParam)
        {
            int x = 100;
                // x := __id [ const ]

            int *ptr = &x;
                // ptr := __id [ const ]

            Console.WriteLine((int)ptr);
                // %0 := System.Console.WriteLine(int) [ System.Console const ]

            Console.WriteLine(*ptr);
                // %1 := System.Console.WriteLine(int) [ System.Console const ]

            ptrField = ptrParam;
                // this.ptrField := __id [ const ]
            uPtrField = uPtrParam;
                // this.uPtrField := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }
        
        [TestMethod]
        public void Assignments_Complex_Chaining()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public Class1[] ArrayProperty { get; set; }
        public Class1[] arrayField;
        public string stringField;

        public void Foo(Class1 other, string s)
        {
            other.ArrayProperty[0].arrayField[1].stringField = s;
                // %0 := Namespace.Class1.ArrayProperty.get [ other ]
                // %1 := __arrayGet [ %0 ]
                // %2 := __id [ %1.arrayField ]
                // %3 := __arrayGet [ %2 ]
                // %3.stringField := __id [ s ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ControllerMethod_Contains_EntryPointWithNoArguments()
        {
            const string code = @"
using System.ComponentModel;
using System.Web.Mvc;
public class Class1 : Controller
{
    private string field;
    public void Foo() // %0 := __entrypoint [  ]
    {
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Bug169_CreationError_RegressionTest_NullRef()
        {
            // SimplCommerce\src\Modules\SimplCommerce.Module.Reviews\Controllers\ReviewApiController.cs :: ChangeStatus
            // SimplCommerce\src\Modules\SimplCommerce.Module.ShoppingCart\Controllers\CartController.cs :: Remove

            // Exception at: CreateFromAttributSyntax->CreateAnnotationCall->CreateFunctionCall->ApplyAsTarget

            const string code = @"
namespace Namespace
{
    using System.Web.Mvc;

    public class FromBodyAttribute : System.Attribute { }

    public class CartController : Controller
    {
        [HttpPost]
        public object Remove([FromBody] long itemId)
        {
            // %0 := __entrypoint [ const ]
            return null;
        }
    }
}
";
            UcfgVerifier.VerifyInstructions(code, "Remove");
        }

        [TestMethod]
        public void Bug170_CreationError_RegressionTest_SequenceContainedNullElement()
        {
            // SimplCommerce.Module.Catalog.Components.CategoryBreadcrumbViewComponent.Invoke(long ?, System.Collections.Generic.IEnumerable<long>)
            // SimplCommerce\src\Modules\SimplCommerce.Module.Shipping\Models\ShippingProvider.cs :: get_OnlyCountryIds
            // SimplCommerce\src\Modules\SimplCommerce.Module.Shipping\Models\ShippingProvider.cs :: get_OnlyStateOrProvinceIds

            // Exception at: UcfgInstructionFactory.CreateFunctionCall

            const string code = @"
namespace SimplCommerce.Module.Shipping.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class ShippingProvider //: EntityBase
    {
        public string OnlyCountryIdsString { get; set; }

        public IList<long> OnlyCountryIds
        {
            get
            {
                if (string.IsNullOrWhiteSpace(OnlyCountryIdsString))
                    // %0 := SimplCommerce.Module.Shipping.Models.ShippingProvider.OnlyCountryIdsString.get [ this ]
                    // %1 := string.IsNullOrWhiteSpace(string) [ string %0 ]
                {
                    return new List<long>();
                        // %2 := new System.Collections.Generic.List<T>
                        // %3 := System.Collections.Generic.List<T>.List() [ %2 ]
                }

                return OnlyCountryIdsString.Split(',')
                        // %4 := SimplCommerce.Module.Shipping.Models.ShippingProvider.OnlyCountryIdsString.get [ this ]
                        // %5 := string.Split(params char[]) [ %4 const ]
                    .Select(long.Parse)
                        // %6 := System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>) [ System.Linq.Enumerable %5 const ]
                    .ToList();
                        // %7 := System.Linq.Enumerable.ToList<TSource>(System.Collections.Generic.IEnumerable<TSource>) [ System.Linq.Enumerable %6 ]
            }
        }
    }
}
";
            UcfgVerifier.VerifyInstructionsForPropertyGetter(code, "OnlyCountryIds");
        }

        [TestMethod]
        public void Bug171_CreationError_RegressionTest_UnexpectedMergedNamespaceSymbol()
        {
            // SimplCommerce\src\Modules\SimplCommerce.Module.PaymentPaypalExpress\Controllers\PaypalExpressController.cs :: GetAccessToken
            // SimplCommerce\src\SimplCommerce.WebHost\Program.cs :: BuildWebHost2

            // At: UcfgExpressionService.Create

            // This code gives a similar repro, except with "SourceNamespaceSymbol" instead of
            // "MergedNamespaceSymbol" (
            const string code = @"

namespace Ns1
{
    namespace Inner
    {
        public class Builder
        {
            public static Builder CreateDefaultBuilder() => null;
        }
    }
}

namespace Ns2
{
    public class Class1
    {
        public void BuildWebHost2(string[] args)
        {
            Ns1.Inner.Builder.CreateDefaultBuilder();
                // %0 := Ns1.Inner.Builder.CreateDefaultBuilder() [ Ns1.Inner.Builder ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "BuildWebHost2");
        }

        [TestMethod]
        public void AddAssignment_LocalVariable()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            localString += (((""foo"")));
                // %0 := __concat [ localString const ]
                // localString := __id [ %0 ]

            localString += localString;
                // %1 := __concat [ localString localString ]
                // localString := __id [ %1 ]

            localString += parameterString;
                // %2 := __concat [ localString parameterString ]
                // localString := __id [ %2 ]

            localString += fieldString;
                // %3 := __id [ this.fieldString ]
                // %4 := __concat [ localString %3 ]
                // localString := __id [ %4 ]

            localString += PropertyString;
                // %5 := Namespace.Class1.PropertyString.get [ this ]
                // %6 := __concat [ localString %5 ]
                // localString := __id [ %6 ]

            localString += localString += localString += ""123"";
                // %7 := __concat [ localString const ]
                // localString := __id [ %7 ]
                // %8 := __concat [ localString localString ]
                // localString := __id [ %8 ]
                // %9 := __concat [ localString localString ]
                // localString := __id [ %9 ]

            values[0] += Passthrough(localString += ""abc"");
                // %10 := __arrayGet [ values ]
                // %11 := __concat [ localString const ]
                // localString := __id [ %11 ]
                // %12 := Namespace.Class1.Passthrough(string) [ this localString ]
                // %13 := __concat [ %10 %12 ]
                // %14 := __arraySet [ values %13 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }


        [TestMethod]
        public void AddAssignment_Parameter()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            parameterString += (((""foo"")));
                // %0 := __concat [ parameterString const ]
                // parameterString := __id [ %0 ]

            parameterString += localString;
                // %1 := __concat [ parameterString localString ]
                // parameterString := __id [ %1 ]

            parameterString += parameterString;
                // %2 := __concat [ parameterString parameterString ]
                // parameterString := __id [ %2 ]

            parameterString += fieldString;
                // %3 := __id [ this.fieldString ]
                // %4 := __concat [ parameterString %3 ]
                // parameterString := __id [ %4 ]

            parameterString += PropertyString;
                // %5 := Namespace.Class1.PropertyString.get [ this ]
                // %6 := __concat [ parameterString %5 ]
                // parameterString := __id [ %6 ]

            parameterString += parameterString += parameterString += ""123"";
                // %7 := __concat [ parameterString const ]
                // parameterString := __id [ %7 ]
                // %8 := __concat [ parameterString parameterString ]
                // parameterString := __id [ %8 ]
                // %9 := __concat [ parameterString parameterString ]
                // parameterString := __id [ %9 ]

            values[0] += Passthrough(parameterString += ""abc"");
                // %10 := __arrayGet [ values ]
                // %11 := __concat [ parameterString const ]
                // parameterString := __id [ %11 ]
                // %12 := Namespace.Class1.Passthrough(string) [ this parameterString ]
                // %13 := __concat [ %10 %12 ]
                // %14 := __arraySet [ values %13 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void AddAssignment_Field()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            fieldString += (((""foo"")));
                // %0 := __id [ this.fieldString ]
                // %1 := __concat [ %0 const ]
                // this.fieldString := __id [ %1 ]

            fieldString += localString;
                // %2 := __id [ this.fieldString ]
                // %3 := __concat [ %2 localString ]
                // this.fieldString := __id [ %3 ]

            fieldString += parameterString;
                // %4 := __id [ this.fieldString ]
                // %5 := __concat [ %4 parameterString ]
                // this.fieldString := __id [ %5 ]

            fieldString += fieldString;
                // %6 := __id [ this.fieldString ]
                // %7 := __id [ this.fieldString ]
                // %8 := __concat [ %6 %7 ]
                // this.fieldString := __id [ %8 ]

            fieldString += PropertyString;
                // %9 := __id [ this.fieldString ]
                // %10 := Namespace.Class1.PropertyString.get [ this ]
                // %11 := __concat [ %9 %10 ]
                // this.fieldString := __id [ %11 ]

            fieldString += fieldString += fieldString += ""123"";
                // %12 := __id [ this.fieldString ]
                // %13 := __id [ this.fieldString ]
                // %14 := __id [ this.fieldString ]
                // %15 := __concat [ %14 const ]
                // this.fieldString := __id [ %15 ]
                // %16 := __id [ this.fieldString ]
                // %17 := __concat [ %13 %16 ]
                // this.fieldString := __id [ %17 ]
                // %18 := __id [ this.fieldString ]
                // %19 := __concat [ %12 %18 ]
                // this.fieldString := __id [ %19 ]

            values[0] += Passthrough(fieldString += ""abc"");
                // %20 := __arrayGet [ values ]
                // %21 := __id [ this.fieldString ]
                // %22 := __concat [ %21 const ]
                // this.fieldString := __id [ %22 ]
                // %23 := Namespace.Class1.Passthrough(string) [ this this.fieldString ]
                // %24 := __concat [ %20 %23 ]
                // %25 := __arraySet [ values %24 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void AddAssignment_Property()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string fieldString;
        public string PropertyString { get; set; }

        public void Foo(string parameterString, string[] values)
        {
            var localString = ""foo"";
                // localString := __id [ const ]

            PropertyString += (((""foo"")));
                // %0 := Namespace.Class1.PropertyString.get [ this ]
                // %1 := __concat [ %0 const ]
                // %2 := Namespace.Class1.PropertyString.set [ this %1 ]

            PropertyString += localString;
                // %3 := Namespace.Class1.PropertyString.get [ this ]
                // %4 := __concat [ %3 localString ]
                // %5 := Namespace.Class1.PropertyString.set [ this %4 ]

            PropertyString += parameterString;
                // %6 := Namespace.Class1.PropertyString.get [ this ]
                // %7 := __concat [ %6 parameterString ]
                // %8 := Namespace.Class1.PropertyString.set [ this %7 ]

            PropertyString += fieldString;
                // %9 := Namespace.Class1.PropertyString.get [ this ]
                // %10 := __id [ this.fieldString ]
                // %11 := __concat [ %9 %10 ]
                // %12 := Namespace.Class1.PropertyString.set [ this %11 ]

            PropertyString += PropertyString;
                // %13 := Namespace.Class1.PropertyString.get [ this ]
                // %14 := Namespace.Class1.PropertyString.get [ this ]
                // %15 := __concat [ %13 %14 ]
                // %16 := Namespace.Class1.PropertyString.set [ this %15 ]

            PropertyString += PropertyString += PropertyString += ""123"";
                // %17 := Namespace.Class1.PropertyString.get [ this ]
                // %18 := Namespace.Class1.PropertyString.get [ this ]
                // %19 := Namespace.Class1.PropertyString.get [ this ]
                // %20 := __concat [ %19 const ]
                // %21 := Namespace.Class1.PropertyString.set [ this %20 ]
                // %22 := __concat [ %18 %21 ]
                // %23 := Namespace.Class1.PropertyString.set [ this %22 ]
                // %24 := __concat [ %17 %23 ]
                // %25 := Namespace.Class1.PropertyString.set [ this %24 ]

            values[0] += Passthrough(PropertyString += ""abc"");
                // %26 := __arrayGet [ values ]
                // %27 := Namespace.Class1.PropertyString.get [ this ]
                // %28 := __concat [ %27 const ]
                // %29 := Namespace.Class1.PropertyString.set [ this %28 ]
                // %30 := Namespace.Class1.Passthrough(string) [ this %29 ]
                // %31 := __concat [ %26 %30 ]
                // %32 := __arraySet [ values %31 ]
        }

        public string Passthrough(string s) => s;
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void AssignmentOperator_UnsupportedTypes()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            var i = 42 + 1;
                // i := __id [ const ]
            i += 1;
                // i := __id [ const ]
            i -= 1;
                // i := __id [ const ]
            i *= 1;
                // i := __id [ const ]
            i /= 1;
                // i := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Operator_UnsupportedTypes()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            int x;
            double d;
            x = 1 + 2;
                // x := __id [ const ]
            x = 1 - 2;
                // x := __id [ const ]
            x = 1 * 2;
                // x := __id [ const ]
            x = 1 / 2;
                // x := __id [ const ]
            d = 1.0 / 2;
                // d := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Operator_SupportedTypes()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string parameterString, Class1 some, Class1 other)
        {
            var result = parameterString == null;
                // %0 := string.operator ==(string, string) [ string parameterString const ]
                // result := __id [ %0 ]
            result = parameterString != null;
                // %1 := string.operator !=(string, string) [ string parameterString const ]
                // result := __id [ %1 ]
            Class1 classResult = some + other;
                // %2 := Namespace.Class1.operator +(Namespace.Class1, Namespace.Class1) [ Namespace.Class1 some other ]
                // classResult := __id [ %2 ]
            classResult = classResult + some + other;
                // %3 := Namespace.Class1.operator +(Namespace.Class1, Namespace.Class1) [ Namespace.Class1 classResult some ]
                // %4 := Namespace.Class1.operator +(Namespace.Class1, Namespace.Class1) [ Namespace.Class1 %3 other ]
                // classResult := __id [ %4 ]
        }
        public static Class1 operator+ (Class1 left, Class1 right)
        {
            return null;
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void UnaryExpressions_AreIgnored()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            int negative = -1;
                // negative := __id [ const ]
            int positive = 1;
                // positive := __id [ const ]
            int result;

            result = +negative;  // result = -1
                // result := __id [ const ]
            result = +positive;  // result = 1
                // result := __id [ const ]

            result = -negative;  // result = 1
                // result := __id [ const ]
            result = -positive   // result = -1
                // result := __id [ const ]

            int count;
            int index = 0;
                // index := __id [ const ]

            count = index++;    // count = 0, index = 1
                // count := __id [ const ]
            count = ++index;    // count = 2, index = 2
                // count := __id [ const ]

            count = index--;    // count = 2, index = 1
                // count := __id [ const ]
            count = --index;    // count = 0, index = 0
                // count := __id [ const ]

            bool b = false;
                // b := __id [ const ]

            b = !b;
                // b := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Indexer_GetAccess()
        {
            const string code = @"
namespace Namespace
{
    using System.Collections.Generic;
    public class Class1
    {
        public void Foo(string s, List<string> list, Class1 myClass)
        {
            var character = s[0];
                // %0 := string.this[int].get [ s const ]
                // character := __id [ %0 ]

            var i = list[0];
                // %1 := System.Collections.Generic.List<T>.this[int].get [ list const ]
                // i := __id [ %1 ]

            var result = myClass[s, 0, 1.0];
                // %2 := Namespace.Class1.this[string, int, double].get [ myClass s const const ]
                // result := __id [ %2 ]
        }

        public string this[string s, int i, double d]
        {
            get { return ""bar""; }
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Indexer_SetAccess()
        {
            const string code = @"
namespace Namespace
{
    using System.Collections.Generic;
    public class Class1
    {
        public void Foo(string s, List<string> list, Class1 myClass)
        {
            list[0] = ""bar"";
                // %0 := System.Collections.Generic.List<T>.this[int].set [ list const ]

            myClass[s, 0, 1.0] = ""bar"";
                // %1 := Namespace.Class1.this[string, int, double].set [ myClass s const const ]
        }

        public string this[string s, int i, double d]
        {
            set {}
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }
    }
}
