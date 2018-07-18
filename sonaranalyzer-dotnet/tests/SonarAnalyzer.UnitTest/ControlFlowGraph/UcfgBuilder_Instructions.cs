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
            a = s;                  // a := __id [ s ]
            a = ""boo""             // a := __id [ const ]
            //var x = new string[5];
            int i;
            i = 5;                  // i := __id [ const ]

            field = s;              // this.field := __id [ s ]
            this.field = s;         // this.field := __id [ s ]

            staticField = s;        // Namespace.Class1.staticField := __id [ s ]
            Class1.staticField = s; // Namespace.Class1.staticField := __id [ s ]
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
            a = b = c = ""foo""     // c := __id [ const ]
                                    // b := __id [ c ]
                                    // a := __id [ b ]
            int x, y, z;
            x = y = z = 5;          // z := __id [ const ]
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
            var x = Bar<string>.Value;  // %0 := Namespace.Bar<T>.Value.get [ Namespace.Bar<T> ]
                                        // x := __id [ %0 ]

            var y = Bar<int>.Value;     // %1 := Namespace.Bar<T>.Value.get [ Namespace.Bar<T> ]
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
                // %3 := Namespace.Class1.Property.get [ this ]
                // %2 := Namespace.Class1.Property.set [ this %3 ]

            Foo(Property);
                // %5 := Namespace.Class1.Property.get [ this ]
                // %4 := Namespace.Class1.Foo(string) [ this %5 ]

            Property = Foo(Property);
                // %7 := Namespace.Class1.Property.get [ this ]
                // %6 := Namespace.Class1.Foo(string) [ this %7 ]
                // %8 := Namespace.Class1.Property.set [ this %6 ]

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
            Property = s;   // ignored, property has no SetMethod
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
            string a = s;           // a := __id [ s ]

            a = s + s;              // %0 := __concat [ s s ]
                                    // a := __id [ %0 ]

            a = s + s + s;          // %1 := __concat [ s s ]
                                    // %2 := __concat [ %1 s ]
                                    // a := __id [ %2 ]

            a = (s + s) + t;        // %3 := __concat [ s s ]
                                    // %4 := __concat [ %3 t ]
                                    // a := __id [ %4 ]

            a = s + 5;              // %5 := __concat [ s const ]
                                    // a := __id [ %5 ]

            a = 5 + s;              // %6 := __concat [ const s ]
                                    // a := __id [ %6 ]

            a = s + o;              // %7 := __concat [ s o ]
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
            a = s.ToLower();                // %0 := string.ToLower() [ s ]
                                            // a := __id [ %0 ]
            a = (s + s).ToLower();          // %1 := __concat [ s s ]
                                            // %2 := string.ToLower() [ %1 ]
                                            // a := __id [ %2 ]
            Bar(s.ToLower());               // %3 := string.ToLower() [ s ]
                                            // %4 := Namespace.Class1.Bar(string) [ this %3 ]
            a = string.IsNullOrEmpty(s);    // %5 := string.IsNullOrEmpty(string) [ string s ];
                                            // a := __id [ %5 ]
            a = A(B(C(s)));                 // %6 := Namespace.Class1.C(string) [ this s ]
                                            // %7 := Namespace.Class1.B(int) [ this %6 ]
                                            // %8 := Namespace.Class1.A(int) [ this %7 ]
                                            // a := __id [ %8 ]
            int x;
            x = O(s);                       // %9 := Namespace.Class1.O(object) [ this s ]
                                            // x := __id [ %9 ]

            this.Bar(s);                    // %10 := Namespace.Class1.Bar(string) [ this s ]
            base.BaseMethod();              // %11 := Namespace.BaseClass.BaseMethod() [ this ]

            var other = new Class1();       // %12 := new Namespace.Class1
                                            // %13 := Namespace.Class1.Class1() [ %12 ]
                                            // other := __id [ %12 ]
            other.Bar(s);                   // %14 := Namespace.Class1.Bar(string) [ other s ]

            Bar(field);                     // %16 := __id [ this.field ]
                                            // %15 := Namespace.Class1.Bar(string) [ this %16 ]

            Bar(other.field);               // %18 := __id [ other.field ]
                                            // %17 := Namespace.Class1.Bar(string) [ this %18 ]

            StaticMethod(s);                // %19 := Namespace.Class1.StaticMethod(string) [ Namespace.Class1 s ]
            Class1.StaticMethod(s);         // %20 := Namespace.Class1.StaticMethod(string) [ Namespace.Class1 s ]

            a = field;                      // a := __id [ this.field ]
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
            c = new Class1(s);                  // %0 := new Namespace.Class1
                                                // %1 := Namespace.Class1.Class1(string) [ %0 s ]
                                                // c := __id [ %0 ]

            c = new Class1();                   // %2 := new Namespace.Class1
                                                // %3 := Namespace.Class1.Class1() [ %2 ]
                                                // c := __id [ %2 ]

            c = new Class1(new Class1(s));      // %4 := new Namespace.Class1
                                                // %5 := Namespace.Class1.Class1(string) [ %4 s ]
                                                // %6 := new Namespace.Class1
                                                // %7 := Namespace.Class1.Class1(Namespace.Class1) [ %6 %4 ]
                                                // c := __id [ %6 ]

            c = new Class1(s)                   // %8 := new Namespace.Class1
            {                                   // %9 := Namespace.Class1.Class1(string) [ %8 s ]
                Property = s,                   // %10 := Namespace.Class1.Property.set [ %8 s ]
            };                                  // c := __id [ %8 ]

            field = new Class1();               // %11 := new Namespace.Class1
                                                // %12 := Namespace.Class1.Class1() [ %11 ]
                                                // this.field := __id [ %11 ]

            this.field = new Class1();          // %13 := new Namespace.Class1
                                                // %14 := Namespace.Class1.Class1() [ %13 ]
                                                // this.field := __id [ %13 ]

            staticField = new Class1();         // %15 := new Namespace.Class1
                                                // %16 := Namespace.Class1.Class1() [ %15 ]
                                                // Namespace.Class1.staticField := __id [ %15 ]

            Class1.staticField = new Class1();  // %17 := new Namespace.Class1
                                                // %18 := Namespace.Class1.Class1() [ %17 ]
                                                // Namespace.Class1.staticField := __id [ %17 ]

            var other = new Class1();           // %19 := new Namespace.Class1
                                                // %20 := Namespace.Class1.Class1() [ %19 ]
                                                // other := __id [ %19 ]

            other.field = this;                 // other.field := __id [ this ]
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
            Do();       // %0 := Namespace.Class1.Do() [ Namespace.Class1 ]
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
        private string Field;
        public void Foo(string s)
        {
            string a;
            a = s.Ext();            // %0 := Namespace.Extensions.Ext(string) [ Namespace.Extensions s ]
                                    // a := __id [ %0 ]
            a = Extensions.Ext(s);  // %1 := Namespace.Extensions.Ext(string) [ Namespace.Extensions s ]
                                    // a := __id [ %1 ]
            a = this.Field.Ext();   // %3 := __id [ this.Field ]
                                    // %2 := Namespace.Extensions.Ext(string) [ Namespace.Extensions %3 ]
                                    // a := __id [ %2 ]
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
            b1.Foo(s);          // %0 := Namespace.IBar.Foo(string) [ b1 s ]
            b2.Foo(s);          // %1 := Namespace.Bar.Foo(string) [ b2 s ]
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
            b1.Foo(s);            // %0 := Namespace.IBar.Foo<T>(T) [ b1 s ]
            b2.Foo(s);            // %1 := Namespace.Bar.Foo<T>(T) [ b2 s ]
            b2.Fooooo(s);         // %2 := Namespace.Bar.Fooooo(string) [ b2 s ]
            b3.Fooooo(s);         // %3 := Namespace.IBar<T>.Fooooo(T) [ b3 s ]
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
            SendEmailAsync(""body"");                   // %0 := Namespace.Class1.SendEmailAsync(string, bool) [ this const ]
            SendEmailAsync(""body"", isHtml: true);     // %1 := Namespace.Class1.SendEmailAsync(string, bool) [ this const const ]
            SendEmailAsync(""body"", isHtml: false);    // %2 := Namespace.Class1.SendEmailAsync(string, bool) [ this const const ]
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
            SendEmailAsync(s, false);                  // %0 := Namespace.Class1.SendEmailAsync(string, bool) [ this s const ]
            SendEmailAsync(body: s, isHtml: false);    // %1 := Namespace.Class1.SendEmailAsync(string, bool) [ this s const ]
            SendEmailAsync(body: s);                   // %2 := Namespace.Class1.SendEmailAsync(string, bool) [ this s ]
            SendEmailAsync(isHtml: false, body: s);    // %3 := Namespace.Class1.SendEmailAsync(string, bool) [ this const s ]
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
                // %7 := Namespace.Class1.StaticProperty.get [ this ]
                // %6 := Namespace.Class1.Bar(int) [ this %7 ]

            Bar(this.Field);
                // %9 := __id [ this.Field ]
                // %8 := Namespace.Class1.Bar(int) [ this %9 ]

            Bar(StaticField);
                // %11 := __id [ Namespace.Class1.StaticField ]
                // %10 := Namespace.Class1.Bar(int) [ this %11 ]

            Bar(Field, Property, this.InstanceMethod(), StaticMethod());
                // %12 := Namespace.Class1.InstanceMethod() [ this ]
                // %13 := Namespace.Class1.StaticMethod() [ Namespace.Class1 ]
                // %15 := __id [ this.Field ]
                // %16 := Namespace.Class1.Property.get [ this ]
                // %14 := Namespace.Class1.Bar(int, int, int, int) [ this %15 %16 %12 %13 ]
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
            GlobalClass.NoOp();                         // %0 := GlobalClass.NoOp() [ GlobalClass ]

            Ns1.OuterClass.NoOp();                      // %1 := Ns1.OuterClass.NoOp() [ Ns1.OuterClass ]

            Ns1.Inner.InnerClass.NoOp();                // %2 := Ns1.Inner.InnerClass.NoOp() [ Ns1.Inner.InnerClass ]

            Ns1.Inner.InnerClass.NestedClass.NoOp();
                // %3 := Ns1.Inner.InnerClass.NestedClass.NoOp() [ Ns1.Inner.InnerClass.NestedClass ]

            NoOp();                                     // %4 := GlobalClass.NoOp() [ GlobalClass ]
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
            i = GlobalClass.Property;                   // %0 := GlobalClass.Property.get [ GlobalClass ]
                                                        // i := __id [ %0 ]

            i = Ns1.OuterClass.Property;                // %1 := Ns1.OuterClass.Property.get [ Ns1.OuterClass ]
                                                        // i := __id [ %1 ]

            i = Ns1.Inner.InnerClass.Property;          // %2 := Ns1.Inner.InnerClass.Property.get [ Ns1.Inner.InnerClass ]
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
            GlobalClass.Property = 42;          // %0 := GlobalClass.Property.set [ GlobalClass const ]

            Ns1.OuterClass.Property = 42;       // %1 := Ns1.OuterClass.Property.set [ Ns1.OuterClass const ]

            Ns1.Inner.InnerClass.Property = 42; // %2 := Ns1.Inner.InnerClass.Property.set [ Ns1.Inner.InnerClass const ]


            Ns1.Inner.InnerClass.NestedClass.Property = 42;
                // %3 := Ns1.Inner.InnerClass.NestedClass.Property.set [ Ns1.Inner.InnerClass.NestedClass const ]

            Property = 42;                      // %4 := GlobalClass.Property.set [ GlobalClass const ]
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
            var variable = Field;   // variable := __id [ this.Field ]

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

            Bar2();     // %2 := Namespace.Class1.Bar2() [ this ]
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
                // %1 := __id [ this.Field ]
                // %0 := string.ToLower() [ %1 ]
                // x := __id [ %0 ]
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
            var x = Bar(s, a => a);     // %0 := Namespace.Class1.Bar(string, System.Func<string, string>) [ this s const ]
                                        // x := __id [ %0 ]

            this.Bar(s, a => a);        // %1 := Namespace.Class1.Bar(string, System.Func<string, string>) [ this s const ]
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
            UcfgVerifier.VerifyInstructions(code, "Foo");
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
            UcfgVerifier.VerifyInstructions(code, "Remove");
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
            var data = itemId;      // data := __id [ const ]
            var data = itemId2;     // data := __id [ itemId2 ]
            return null;
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
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
        string a = ""a""; // a := __id [ const ]
        string b = ""b""; // b := __id [ const ]
        string c = ""c""; // c := __id [ const ]
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
        public void Regression_SyntaxNode_Not_In_CFG()
        {
            const string code = @"
using System;

public class Foo
{
    public void Bar(bool b)
    {
        // The ternary operator is not walked because it is a Jump node of a block
        // that's why when we create instruction for the variable declarator we
        // get NRE for the assignment argument.
        var s = b ? ""s1"" : ""s2""; // s := __id [ __unknown ]
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Bar");
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
            bool @bool = true;      // bool := __id [ const ]
            byte @byte = 1;         // byte := __id [ const ]
            sbyte @sbyte = 1;       // sbyte := __id [ const ]
            char @char = 'c';       // char := __id [ const ]
            decimal @decimal = 1;   // decimal := __id [ const ]
            double @double = 1;     // double := __id [ const ]
            float @float = 1;       // float := __id [ const ]
            int @int = 1;           // int := __id [ const ]
            uint @uint = 1;         // uint := __id [ const ]
            long @long = 1;         // long := __id [ const ]
            ulong @ulong = 1;       // ulong := __id [ const ]
            short @short = 1;       // short := __id [ const ]
            ushort @ushort = 1;     // ushort := __id [ const ]

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
                // %1 := Namespace.Class1.Property.get [ this ]
                // %0 := Namespace.Class1.Property.get [ %1 ]
                // %2 := new Namespace.Class1
                // %3 := Namespace.Class1.Class1() [ %2 ]
                // %4 := Namespace.Class1.Property.set [ %0 %2 ]

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
            int x = 100;                    // x := __id [ const ]
            int *ptr = &x;                  // ptr := __id [ const ]
            Console.WriteLine((int)ptr);    // %0 := System.Console.WriteLine(int) [ System.Console const ]
            Console.WriteLine(*ptr);        // %1 := System.Console.WriteLine(int) [ System.Console const ]

            ptrField = ptrParam;            // this.ptrField := __id [ const ]
            uPtrField = uPtrParam;          // this.uPtrField := __id [ const ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Array_Get()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s, string[] a, string [][] jagged, string[,] multi)
        {
            s = a[0];           // %0 := __arrayGet [ a ]
                                // s := __id [ %0 ]

            s = ((a[0]));       // %1 := __arrayGet [ a ]
                                // s := __id [ %1 ]

            Bar(a[0]);          // %2 := __arrayGet [ a ]
                                // %3 := Namespace.Class1.Bar(string) [ this %2 ]

            s = jagged[0][0];   // %4 := __arrayGet [ jagged ]
                                // %5 := __arrayGet [ %4 ]
                                // s := __id [ %5 ]

            s = multi[0, 0];    // %6 := __arrayGet [ multi ]
                                // s := __id [ %6 ]
        }

        public void Bar(string s) {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void Assignments_Array_Set()
        {
            const string code = @"
using System.Collections.Generic;
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s, string[] a, string[][] jagged, string[,] multi, List<string> list)
        {
            a[0] = s;           // %0 := __arraySet [ a s ]

            ((a[0])) = s;       // %1 := __arraySet [ a s ]

            jagged[0][0] = s;   // %2 := __arrayGet [ jagged ]
                                // %3 := __arraySet [ %2 s ]

            multi[0, 0] = s;    // %4 := __arraySet [ multi s ]

            ((jagged[0]))[0] = s;   // %5 := __arrayGet [ jagged ]
                                    // %6 := __arraySet [ %5 s ]

            a[0] = a[1];        // %7 := __arrayGet [ a ]
                                // %8 := __arraySet [ a %7 ]

            // Strings and lists have indexers but should not be handled as arrays;
            // until collection indexers are supported, the string and list element
            // access is represented as constant
            var c = s[0];       // c := __id [ const ]
            var i = list[0];    // i := __id [ const ]
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
                // %3 := __id [ %1.arrayField ]
                // %2 := __arrayGet [ %3 ]
                // %2.stringField := __id [ s ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_WithNew()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            // Primitive type
            var x0 = new string[42];    // %0 := new string[]
                                        // x0 := __id [ %0 ]

            // Array of objects
            var x1 = new Class1[0];     // %1 := new Namespace.Class1[]
                                        // x1 := __id [ %1 ]

            // Multi-rank arrays
            var x2 = new int[10,2];     // %2 := new int[*,*]
                                        // x2 := __id [ %2 ]

            // Array with initializer (initializer is ignored currently - issue #161)
            var x3 = new string[] { ""aaa"", ""bbb"", ""ccc"" };    // %3 := new string[]
                                                                    // x3 := __id [ %3 ]
        }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_WithNew_NonExistentType()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            // References a type that does not exist
            // This doesn't fail as we can still get the array type symbol,
            // even though the ElementKind is unknown.
            var x0 = new NonExistentType[42];   // %0 := new NonExistentType[]
                                                // x0 := __id [ %0 ]
        }
    }
}";

            UcfgVerifier.VerifyInstructions(code, "Foo");
        }

        [TestMethod]
        public void ArrayCreation_WithCreateInstance()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            var a = System.Array.CreateInstance(typeof(string), 10);    // %0 := System.Array.CreateInstance(System.Type, int) [ System.Array const const ]
                                                                        // a := __id [ %0 ]
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
                    // %1 := SimplCommerce.Module.Shipping.Models.ShippingProvider.OnlyCountryIdsString.get [ this ]
                    // %0 := string.IsNullOrWhiteSpace(string) [ string %1 ]
                {
                    return new List<long>();
                        // %2 := new System.Collections.Generic.List<T>
                        // %3 := System.Collections.Generic.List<T>.List() [ %2 ]
                }

                return OnlyCountryIdsString.Split(',')
                        // %5 := SimplCommerce.Module.Shipping.Models.ShippingProvider.OnlyCountryIdsString.get [ this ]
                        // %4 := string.Split(params char[]) [ %5 const ]
                    .Select(long.Parse)
                        // %6 := System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>) [ System.Linq.Enumerable %4 const ]
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
        public void Dynamic()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public string Foo(Class1 c)
        {
            dynamic dyn = c.Bar;
                // dyn := __id [ __unknown ]

            if (dyn.User != null)
            {
                return ""bar"";
            }

            return c.ToString();
                // %0 := object.ToString() [ c ]
        }
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }
    }
}
