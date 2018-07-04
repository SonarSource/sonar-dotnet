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
using SonarAnalyzer.UnitTest.Security.Framework;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
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
        private string field;
        public void Foo(string s)
        {
            string a, b;
            a = s;                  // a := __id [ s ]
            a = ""boo""             // a := __id [ const ]
            var x = new string[5];  // x := __id [ const ]
            int i;
            i = 5;                  // i := __id [ const ]
            field = s;              // ignored
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
        public void Assignments_Static_Poperty_On_Generic_Class()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
        {
            var x = Bar<string>.Value;  // x := __id [ const ]
            var y = Bar<int>.Value;     // y := __id [ const ]
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
        public string Property { get; set; }
        public int IntProperty { get; set; }
        public object ObjectProperty { get; set; }
        public string Foo(string s)
        {
            string a;
            Property = s;                           // %0 := Namespace.Class1.Property.set [ this s ]
            a = Property;                           // %1 := Namespace.Class1.Property.get [ this ]
                                                    // a := __id [ %1 ]
            Property = Property;                    // %2 := Namespace.Class1.Property.get [ this ]
                                                    // %3 := Namespace.Class1.Property.set [ this %2 ]
            Foo(Property);                          // %4 := Namespace.Class1.Property.get [ this ]
                                                    // %5 := Namespace.Class1.Foo(string) [ this %4 ]
            Property = Foo(Property);               // %6 := Namespace.Class1.Property.get [ this ]
                                                    // %7 := Namespace.Class1.Foo(string) [ this %6 ]
                                                    // %8 := Namespace.Class1.Property.set [ this %7 ]
            ObjectProperty = s;                     // %9 := Namespace.Class1.ObjectProperty.set [ this s ]
            ObjectProperty = 5;                     // %10 := Namespace.Class1.ObjectProperty.set [ this const ]
            var x = IntProperty = 5;                // %11 := Namespace.Class1.IntProperty.set [ this const ]
                                                    // x := __id [ %11 ]

            this.Property = s                       // %12 := Namespace.Class1.Property.set [ this s ]

            var other = new Class1();               // %13 := new Namespace.Class1
                                                    // %14 := Namespace.Class1.Class1() [ %13 ]
                                                    // other := __id [ %13 ]
            other.Property = s;                     // %15 := Namespace.Class1.Property.set [ other s ]
            other.ObjectProperty = other.Property;  // %16 := Namespace.Class1.ObjectProperty.set [ other const ]

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
        public void Foo(string s, string t)
        {
            string a = s;           // a := __id [ s ]
            a = s + s;              // %0 := __concat [ s s ]
                                    // a := __id [ %0 ]
            a = s + s + s;          // %1 := __concat [ s s ]
                                    // %2 := __concat [ s %1 ]
                                    // a := __id [ %2 ]
            a = (s + s) + t;        // %3 := __concat [ s s ]
                                    // %4 := __concat [ t %3 ]
                                    // a := __id [ %4 ]
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

            Bar(field);                     // %15 := Namespace.Class1.Bar(string) [ this const ]
            Bar(other.field);               // %16 := Namespace.Class1.Bar(string) [ this const ]
        }
        public void Bar(string s) { }
        public string A(int x) { return x.ToString(); }
        public int B(int x) { return x; }
        public int C(string s) { 5; }
        public int O(object o) { 5; }
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
        public string Property { get; set; }
        public Class1() { }
        public Class1(string s) { }
        public Class1(Class1 other) { }
        public void Foo(string s)
        {
            Class1 c;
            c = new Class1(s);              // %0 := new Namespace.Class1
                                            // %1 := Namespace.Class1.Class1(string) [ %0 s ]
                                            // c := __id [ %0 ]

            c = new Class1();               // %2 := new Namespace.Class1
                                            // %3 := Namespace.Class1.Class1() [ %2 ]
                                            // c := __id [ %2 ]

            c = new Class1(new Class1(s));  // %4 := new Namespace.Class1
                                            // %5 := Namespace.Class1.Class1(string) [ %4 s ]
                                            // %6 := new Namespace.Class1
                                            // %7 := Namespace.Class1.Class1(Namespace.Class1) [ %6 %4 ]
                                            // c := __id [ %6 ]

            c = new Class1(s)               // %8 := new Namespace.Class1
            {                               // %9 := Namespace.Class1.Class1(string) [ %8 s ]
                Property = s,               // %10 := Namespace.Class1.Property.set [ %8 s ]
            };                              // c := __id [ %8 ]
        }
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
            a = s.Ext();            // %0 := Namespace.Extensions.Ext(string) [ Namespace.Extensions s ]
                                    // a := __id [ %0 ]
            a = Extensions.Ext(s);  // %1 := Namespace.Extensions.Ext(string) [ Namespace.Extensions s ]
                                    // a := __id [ %1 ]
            a = this.field.Ext();   // %2 := Namespace.Extensions.Ext(string) [ Namespace.Extensions const ]
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
        public void ControllerMethod_Contains_EntryPoint_And_Attributes()
        {
            const string code = @"
using System.ComponentModel;
using System.Web.Mvc;
public class Class1 : Controller
{
    private string field;
    public void Foo([Description]string s, [Missing]string x)
    {               // %0 := __entrypoint [ s x ]
    }               // %1 := System.ComponentModel.DescriptionAttribute.DescriptionAttribute() [  ]
                    // s := __annotation [ %1 ]
                    // the other attribute is unknown and is not included
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
        var s = b ? ""s1"" : ""s2"";
    }
}";
            UcfgVerifier.GetUcfgForMethod(code, "Bar");
        }

        [TestMethod]
        public void BuiltInTypeAreConvertedToConstant()
        {
            const string code = @"
namespace Namespace
{
    public class Class1
    {
        public void Foo()
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
        }

        public void Bar(object o) {}
    }
}";
            UcfgVerifier.VerifyInstructions(code, "Foo");
        }
    }
}
