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
using System.Linq;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Protobuf.Ucfg;
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

            var x = new string[5];

            int i;
            i = 5;                  // ignored
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

            int x, y, x;
            x = y = z = 5;          // ignored
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
            var x = Bar(s, a => a);     // %0 := Namespace.Class1.Bar(string, System.Func<string, string>) [ s const ]
                                        // x := __id [ %0 ]
        }

        public string Bar(string s, Func<string, string> a)
        {
            return s;
        }
    }
}";
            UcfgVerifier.GetUcfgForMethod(code, "Foo");
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
            var y = Bar<int>.Value;     // ignored
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
        private string Property { get; set; }
        private int IntProperty { get; set; }
        private object ObjectProperty { get; set; }

        public string Foo(string s)
        {
            string a;

            Property = s;       // %0 := Namespace.Class1.Property.set [ s ]
            a = Property;       // %1 := Namespace.Class1.Property.get [  ]
                                // a := __id [ %1 ]

            Property = Property;// %2 := Namespace.Class1.Property.get [  ]
                                // %3 := Namespace.Class1.Property.set [ %2 ]

            Foo(Property);      // %4 := Namespace.Class1.Property.get [  ]
                                // %5 := Namespace.Class1.Foo(string) [ %4 ]

            Property = Foo(Property);   // %6 := Namespace.Class1.Property.get [  ]
                                        // %7 := Namespace.Class1.Foo(string) [ %6 ]
                                        // %8 := Namespace.Class1.Property.set [ %7 ]

            ObjectProperty = s;         // %9 := Namespace.Class1.ObjectProperty.set [ s ]

            ObjectProperty = 5;         // ignored

            var x = IntProperty = 5;    // ignored

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

            int x, y, z;
            x = y = z = 5;          // ignored
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
    public class Class1
    {
        private string field;
        public void Foo(string s)
        {
            string a;

            a = s.ToLower();        // %0 := string.ToLower() [ s ]
                                    // a := __id [ %0 ]

            a = (s + s).ToLower();  // %1 := __concat [ s s ]
                                    // %2 := string.ToLower() [ %1 ]
                                    // a := __id [ %2 ]

            Bar(s.ToLower());       // %3 := string.ToLower() [ s ]
                                    // %4 := Namespace.Class1.Bar(string) [ %3 ]

            a = string.IsNullOrEmpty(s);    // %5 := string.IsNullOrEmpty(string) [ s ];
                                            // a := __id [ const ] // not using %5 because it is not string

            a = A(B(C(s)));         // %6 := Namespace.Class1.C(string) [ s ]
                                    // B is ignored
                                    // %7 := Namespace.Class1.A(int) [ const ]
                                    // a := __id [ %7 ]

            int x;
            x = O(s);               // %8 := Namespace.Class1.O(object) [ s ];
                                    // no assignment is added because O returns int
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
            c = new Class1(s);              // %0 := Namespace.Class1.Class1(string) [ s ]

            c = new Class1();               // ignored, does not accept or return string

            c = new Class1(new Class1(s));  // %1 := Namespace.Class1.Class1(string) [ s ]

            c = new Class1(s)               // %2 := Namespace.Class1.Class1(string) [ s ]
            {
                Property = s,               // %3 := Namespace.Class1.Property.set [ s ]
            };
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

            a = s.Ext();            // %0 := Namespace.Extensions.Ext(string) [ s ]
                                    // a := __id [ %0 ]

            a = Extensions.Ext(s);  // %1 := Namespace.Extensions.Ext(string) [ s ]
                                    // a := __id [ %1 ]

            a = this.field.Ext();   // %2 := Namespace.Extensions.Ext(string) [ const ]
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
            b1.Foo(s);          // %0 := Namespace.IBar.Foo(string) [ s ]
            b2.Foo(s);          // %1 := Namespace.Bar.Foo(string) [ s ]
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
            b1.Foo(s);            // %0 := Namespace.IBar.Foo<T>(T) [ s ]
            b2.Foo(s);            // %1 := Namespace.Bar.Foo<T>(T) [ s ]
            b2.Fooooo(s);         // %2 := Namespace.Bar.Fooooo(string) [ s ]
            b3.Fooooo(s);         // %3 := Namespace.IBar<T>.Fooooo(T) [ s ]
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

            var a = ucfg.BasicBlocks[0].Instructions[0].Args[0];
            var b = ucfg.BasicBlocks[0].Instructions[1].Args[0];
            var c = ucfg.BasicBlocks[0].Instructions[2].Args[0];

            // The constant expressions share the same instance of the Const value
            // for performance and simplicity. The protobuf serializer will deserialize
            // the values as a singleton again.
            a.Should().Be(b);
            a.Should().Be(c);
        }

        private static void ValidateInstruction(Instruction instruction, string methodId, string variable, params string[] args)
        {
            instruction.MethodId.Should().Be(methodId);
            instruction.Variable.Should().Be(variable);
            instruction.Location.Should().NotBeNull();
            if (args.Length > 0)
            {
                instruction.Args.Select(x => x.Var?.Name ?? x.Const?.Value).ShouldBeEquivalentTo(args, o => o.WithStrictOrdering());
            }
            else
            {
                instruction.Args.Should().BeEmpty();
            }
        }

        private static UCFG CreateUcfgForConstructor(string code, string name)
        {
            var (syntaxTree, semanticModel) = TestHelper.Compile(code, Verifier.SystemWebMvcAssembly);

            var ctor = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == name);

            var builder = new UniversalControlFlowGraphBuilder();

            var ucfg = builder.Build(semanticModel, ctor,
                semanticModel.GetDeclaredSymbol(ctor), CSharpControlFlowGraph.Create(ctor.Body, semanticModel));
            return ucfg;
        }
    }
}
