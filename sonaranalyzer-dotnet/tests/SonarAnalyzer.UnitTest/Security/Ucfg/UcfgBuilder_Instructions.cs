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
using csharp::SonarAnalyzer.Security;
using csharp::SonarAnalyzer.Security.Ucfg;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Protobuf.Ucfg;
using SonarAnalyzer.SymbolicExecution.ControlFlowGraph;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class UcfgBuilder_Instructions : UcfgBuilderTestBase
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
            a = s;                  // a = __id(s)
            a = ""boo""             // a = __id("")

            var x = new string[5];

            int i;
            i = 5;                  // ignored
            field = s;              // ignored
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "\"\"" })
                );
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

            a = b = c = ""foo""     // c = __id("")
                                    // b = __id(c)
                                    // a = __id(b)

            int x, y, x;
            x = y = z = 5;          // ignored
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");
            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, KnownMethodId.Assignment, "c", new[] { "\"\"" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "b", new[] { "c" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "b" })
                );
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
            var x = Bar(s, a => a);     // %0 = Bar(s, const)
                                        // x = __id(%0)
        }

        public string Bar(string s, Func<string, string> a)
        {
            return s;
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");
            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, "Namespace.Class1.Bar(string, System.Func<string, string>)",
                    "%0", new[] { "s", "\"\"" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "x", new[] { "%0" })
                );
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
            var x = Bar<string>.Value;  // x = __id("")
            var y = Bar<int>.Value;     // ignored
        }
    }
    public class Bar<T>
    {
        public static T Value { get { return default(T); } }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");
            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, KnownMethodId.Assignment, "x", new[] { "\"\"" })
                );
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

            Property = s;       // %0 = Class1.Property.set(s)
            a = Property;       // %1 = Class1.Property.get()
                                // a = __id(%1)

            Property = Property;// %2 = Class1.Property.get()
                                // %3 = Class1.Property.set(%2)

            Foo(Property);      // %4 = Class1.Property.get()
                                // %5 = Class1.Foo(%4)

            Property = Foo(Property);   // %6 = Class1.Property.get()
                                        // %7 = Foo(%6)
                                        // %8 = Class1.Property.set(%7)

            ObjectProperty = s;         // $9 = Class1.ObjectProperty.set(s)

            ObjectProperty = 5;         // ignored

            var x = IntProperty = 5;    // ignored

            return s;
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);

            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, "Namespace.Class1.Property.set", "%0", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Class1.Property.get", "%1"),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%1" }),

                i => ValidateInstruction(i, "Namespace.Class1.Property.get", "%2"),
                i => ValidateInstruction(i, "Namespace.Class1.Property.set", "%3", new[] { "%2" }),

                i => ValidateInstruction(i, "Namespace.Class1.Property.get", "%4"),
                i => ValidateInstruction(i, "Namespace.Class1.Foo(string)", "%5", new[] { "%4" }),

                i => ValidateInstruction(i, "Namespace.Class1.Property.get", "%6"),
                i => ValidateInstruction(i, "Namespace.Class1.Foo(string)", "%7", new[] { "%6" }),
                i => ValidateInstruction(i, "Namespace.Class1.Property.set", "%8", new[] { "%7" }),

                i => ValidateInstruction(i, "Namespace.Class1.ObjectProperty.set", "%9", new[] { "s" })
                );
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
            var ucfg = CreateUcfgForConstructor(code, "Class1");

            ucfg.BasicBlocks.Should().HaveCount(2);

            AssertCollection(ucfg.BasicBlocks[0].Instructions);
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
            string a = s;           // a = __id(s)

            a = s + s;              // %0 = __concat(s, s)
                                    // a = __id(%0)

            a = s + s + s;          // %1 = __concat(s, s)
                                    // %2 = __concat(s, %1)
                                    // a = __id(%2)

            a = (s + s) + t;        // %3 = __concat(s, s)
                                    // %4 = __concat(%3, t)
                                    // a = __id(%4)

            int x, y, z;
            x = y = z = 5;          // ignored
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Concatenation, "%0", new[] { "s", "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%0" }),
                i => ValidateInstruction(i, KnownMethodId.Concatenation, "%1", new[] { "s", "s" }),
                i => ValidateInstruction(i, KnownMethodId.Concatenation, "%2", new[] { "s", "%1" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%2" }),
                i => ValidateInstruction(i, KnownMethodId.Concatenation, "%3", new[] { "s", "s" }),
                i => ValidateInstruction(i, KnownMethodId.Concatenation, "%4", new[] { "t", "%3" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%4" })
                );
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

            a = s.ToLower();        // %0 = string.ToLower(s)
                                    // a = __id(%0)

            a = (s + s).ToLower();  // %1 = __concat(s, s)
                                    // %2 = string.ToLower(%1)
                                    // a = __id(%2)

            Bar(s.ToLower());       // %3 = string.ToLower(s)
                                    // %4 = Bar(%3)

            a = string.IsNullOrEmpty(s);    // %5 = string.IsNullOrEmpty(s);
                                            // a = _id(const) // not using %5 because it is not string

            a = A(B(C(s)));         // %6 = C(s)
                                    // B is ignored
                                    // %7 = A("")
                                    // a = __id(%7)

            int x;
            x = O(s);               // %8 = O(s);
                                    // no assignment is added because O returns int
        }

        public void Bar(string s) { }

        public string A(int x) { return x.ToString(); }
        public int B(int x) { return x; }
        public int C(string s) { 5; }
        public int O(object o) { 5; }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, "string.ToLower()", "%0", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%0" }),
                i => ValidateInstruction(i, KnownMethodId.Concatenation, "%1", new[] { "s", "s" }),
                i => ValidateInstruction(i, "string.ToLower()", "%2", new[] { "%1" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%2" }),
                i => ValidateInstruction(i, "string.ToLower()", "%3", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Class1.Bar(string)", "%4", new[] { "%3" }),
                i => ValidateInstruction(i, "string.IsNullOrEmpty(string)", "%5", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "\"\"" }),
                i => ValidateInstruction(i, "Namespace.Class1.C(string)", "%6", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Class1.A(int)", "%7", new[] { "\"\"" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%7" }),
                i => ValidateInstruction(i, "Namespace.Class1.O(object)", "%8", new[] { "s" })
                );
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
            c = new Class1(s);              // %0 = Class1(s)

            c = new Class1();               // ignored, does not accept or return string

            c = new Class1(new Class1(s));  // %1 = Class1(s)

            c = new Class1(s)               // %2 = Class1(s)
            {
                Property = s,               // %3 = Property.set(s)
            };
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, "Namespace.Class1.Class1(string)", "%0", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Class1.Class1(string)", "%1", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Class1.Class1(string)", "%2", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Class1.Property.set", "%3", new[] { "s" })
                );
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

            a = s.Ext();            // %0 = Extensions.Ext(s);
                                        // a = __id(%0)

            a = Extensions.Ext(s);  // %1 = Extensions.Ext(s);
                                        // a = __id(%1)

            a = this.field.Ext();   // %2 = Extensions.Ext(const);
                                        // a = __id(%2)
        }
    }

    public static class Extensions
    {
        public static string Ext(this string s) { return s; }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, "Namespace.Extensions.Ext(string)", "%0", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%0" }),
                i => ValidateInstruction(i, "Namespace.Extensions.Ext(string)", "%1", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%1" }),
                i => ValidateInstruction(i, "Namespace.Extensions.Ext(string)", "%2", new[] { "\"\"" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%2" })
                );
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
            b2.Foo(s);
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
            var ucfg = GetUcfgForMethod(code, "Foobar");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, "Namespace.IBar.Foo(string)", "%0", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Bar.Foo(string)", "%1", new[] { "s" })
                );
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
            b2.Foo(s);
            b2.Fooooo(s);
            b3.Fooooo(s);
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
            var ucfg = GetUcfgForMethod(code, "Foobar");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, "Namespace.IBar.Foo<T>(T)", "%0", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Bar.Foo<T>(T)", "%1", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Bar.Fooooo(string)", "%2", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.IBar<T>.Fooooo(T)", "%3", new[] { "s" })
                );
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
    {               //      %0 = __entrypoint(s, x)
    }               //      %1 = Description()
                    //      s = __annotation(%1)
                    // the other attribute is unknown and is not included
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[1].Instructions,
                i => ValidateInstruction(i, KnownMethodId.EntryPoint, "%0", new[] { "s", "x" }),
                i => ValidateInstruction(i, "System.ComponentModel.DescriptionAttribute.DescriptionAttribute()", "%1"),
                i => ValidateInstruction(i, KnownMethodId.Annotation, "s", new[] { "%1" })
                );
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
        string a = ""a""; // a = __id(const)
        string b = ""b""; // b = __id(const)
        string c = ""c""; // c = __id(const)
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

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
