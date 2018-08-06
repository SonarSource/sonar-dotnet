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
    public class BaseClass
    {
        public string baseField;
    }

    public class Class1 : BaseClass
    {
        public string field;

        public void Foo()
        {
            var variable = field;
                // %0 := __id [ this.field ]
                // variable := __id [ %0 ]
             variable = this.field;
                // %1 := __id [ this.field ]
                // variable := __id [ %1 ]
             variable = baseField;
                // %2 := __id [ this.baseField ]
                // variable := __id [ %2 ]
             variable = base.baseField;
                // %3 := __id [ this.baseField ]
                // variable := __id [ %3 ]
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
    }
}
