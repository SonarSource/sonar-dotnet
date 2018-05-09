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
using System;
using System.Collections.Generic;
using System.Linq;
using csharp::SonarAnalyzer.Security.Ucfg;
using csharp::SonarAnalyzer.SymbolicExecution.ControlFlowGraph;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Protobuf.Ucfg;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class UniversalControlFlowGraphBuilder_Instructions
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
using System.Linq;
namespace Namespace
{
    public class Class1
    {
        public void Foo(string s)
        {
            var x = s.Count(c => c == 'a');
            var y = s.Count((c) => c == 'b');
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");
            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, Enumerable_Count_Id, "%0", new[] { "s", "\"\"" }),
                i => ValidateInstruction(i, Enumerable_Count_Id, "%1", new[] { "s", "\"\"" })
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
                i => ValidateInstruction(i, "Namespace.Class1.Property.set", "%8", new[] { "%7" })
                );
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
        }

        public void Bar(string s) { }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.BasicBlocks.Should().HaveCount(2);
            AssertCollection(ucfg.BasicBlocks[0].Instructions,
                i => ValidateInstruction(i, String_ToLower_Id, "%0", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%0" }),
                i => ValidateInstruction(i, KnownMethodId.Concatenation, "%1", new[] { "s", "s" }),
                i => ValidateInstruction(i, String_ToLower_Id, "%2", new[] { "%1" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "%2" }),
                i => ValidateInstruction(i, String_ToLower_Id, "%3", new[] { "s" }),
                i => ValidateInstruction(i, "Namespace.Class1.Bar(string)", "%4", new[] { "%3" }),
                i => ValidateInstruction(i, String_IsNullOrEmpty_Id, "%5", new[] { "s" }),
                i => ValidateInstruction(i, KnownMethodId.Assignment, "a", new[] { "\"\"" })
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

        private UCFG GetUcfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(methodName);

            var builder = new UniversalControlFlowGraphBuilder(semanticModel,
                CSharpControlFlowGraph.Create(method.Body, semanticModel));

            return builder.Build(method, semanticModel.GetDeclaredSymbol(method));
        }

        private static void AssertCollection<T>(IList<T> items, params Action<T>[] asserts)
        {
            items.Should().HaveSameCount(asserts);
            for (var i = 0; i < items.Count; i++)
            {
                asserts[i](items[i]);
            }
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

        private const string StringId = "string";
        private const string String_ToLower_Id = "string.ToLower()";
        private const string String_IsNullOrEmpty_Id = "string.IsNullOrEmpty(string)";
        private const string Enumerable_Select_Id = "System.Linq.Enumerable.Select<TSource, TResult>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, TResult>)";
        private const string Enumerable_Count_Id = "System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";
    }
}
