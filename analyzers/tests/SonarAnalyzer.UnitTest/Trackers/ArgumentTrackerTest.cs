/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Collections;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;
using SonarAnalyzer.Trackers;
using SonarAnalyzer.UnitTest.Trackers;

namespace SonarAnalyzer.UnitTest.Trackers;

[TestClass]
public class ArgumentTrackerTest
{
    [TestMethod]
    public void Method_SimpleArgument()
    {
        var snippet = """
            System.IFormatProvider provider = null;
            1.ToString($$provider);
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""1.ToString($$provider);""", 0, true)]
    [DataRow("""1.ToString($$provider);""", 1, false)]
    [DataRow("""1.ToString("", $$provider);""", 1, true)]
    [DataRow("""1.ToString("", $$provider);""", 0, false)]
    [DataRow("""1.ToString("", $$provider: provider);""", 1, true)]
    [DataRow("""1.ToString("", $$provider: provider);""", 0, true)]
    [DataRow("""1.ToString($$provider: provider, format: "");""", 1, true)]
    [DataRow("""1.ToString($$provider: provider, format: "");""", 0, true)]
    public void Method_Position(string invocation, int position, bool expected)
    {
        var snippet = $$"""
            System.IFormatProvider provider = null;
            {{invocation}}
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", position);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""int.TryParse("", $$out var result);""")]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""")]
    public void Method_RefOut_True(string invocation)
    {
        var snippet = $$"""
            System.IFormatProvider provider = null;
            {{invocation}}
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", x => true, RefKind.Out);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""int.TryParse("", $$out var result);""", RefKind.Ref)]
    [DataRow("""int.TryParse($$"", out var result);""", RefKind.Out)]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""", RefKind.Ref)]
    [DataRow("""int.TryParse("", $$System.Globalization.NumberStyles.HexNumber, null, out var result);""", RefKind.Out)]
    public void Method_RefOut_False(string invocation, RefKind refKind)
    {
        var snippet = $$"""
            System.IFormatProvider provider = null;
            {{invocation}}
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", x => true, refKind);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("""int.TryParse("", $$out var result);""")]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""")]
    public void Method_RefOut_Unspecified(string invocation)
    {
        var snippet = $$"""
            System.IFormatProvider provider = null;
            {{invocation}}
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", x => true);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""new Direct().M($$1);""", true)]
    [DataRow("""new DirectDifferentParameterName().M($$1);""", false)] // FN. This would require ExplicitOrImplicitInterfaceImplementations from the internal ISymbolExtensions in Roslyn.
    [DataRow("""(new Explicit() as I).M($$1);""", true)]
    [DataRow("""(new ExplicitDifferentParameterName() as I).M($$1);""", true)]
    public void Method_Inheritance_Interface(string invocation, bool expected)
    {
        var snippet = $$"""
            interface I
            {
                void M(int parameter);
            }
            public class Direct: I
            {
                public void M(int parameter) { }
            }
            public class DirectDifferentParameterName: I
            {
                public void M(int renamed) { }
            }
            public class Explicit: I
            {
                void I.M(int parameter) { }
            }
            public class ExplicitDifferentParameterName: I
            {
                void I.M(int renamed) { }
            }
            public class Test
            {
                void M()
                {
                    {{invocation}}
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(m => true, (m, c) => m.Equals("M", c), p => p.Name == "parameter", x => true, null);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""comparer.Compare($$default, default);""")]
    [DataRow("""new MyComparer<int>().Compare($$1, 2);""")]
    public void Method_Inheritance_BaseClasses_Generics(string invocation)
    {
        var snippet = $$"""
            using System.Collections.Generic;
            public class MyComparer<T> : Comparer<T>
            {
                public MyComparer() { }
                public override int Compare(T a, T b) => 1;
            }
            public class Test
            {
                void M<T>(MyComparer<T> comparer)
                {
                    {{invocation}}
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_Generic_Comparer_T, "Compare", "x", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""OnInsert($$1, null);""")]
    [DataRow("""OnInsert(position: $$1, null);""")]
    public void Method_Inheritance_BaseClasses_Overrides(string invocation)
    {
        var snippet = $$"""
            using System.Collections;
            public class Collection<T> : CollectionBase
            {
                protected override void OnInsert(int position, object value) { }

                void M(T arg)
                {
                    {{invocation}}
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_CollectionBase, "OnInsert", "index", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""ProcessStartInfo($$"fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo($$"fileName")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 0, false)]
    [DataRow("""ProcessStartInfo($$"fileName", "arguments")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo(arguments: $$"arguments", fileName: "fileName")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo(arguments: $$"arguments", fileName: "fileName")""", "fileName", 0, false)]
    [DataRow("""ProcessStartInfo(arguments: "arguments", $$fileName: "fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo(arguments: "arguments", $$fileName: "fileName")""", "arguments", 1, false)]
    public void Constructor_SimpleArgument(string constructor, string parameterName, int argumentPosition, bool expected)
    {
        var snippet = $$"""
            _ = new System.Diagnostics.{{constructor}};
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Diagnostics_ProcessStartInfo, parameterName, argumentPosition);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""($$"fileName")""", "fileName", 0, true)]
    [DataRow("""($$"fileName")""", "arguments", 1, false)]
    [DataRow("""("fileName", $$"arguments")""", "arguments", 1, true)]
    [DataRow("""("fileName", $$"arguments")""", "arguments", 0, false)]
    [DataRow("""($$"fileName", "arguments")""", "arguments", 1, false)]
    [DataRow("""(arguments: $$"arguments", fileName: "fileName")""", "arguments", 1, true)]
    [DataRow("""(arguments: $$"arguments", fileName: "fileName")""", "fileName", 0, false)]
    [DataRow("""(arguments: "arguments", $$fileName: "fileName")""", "fileName", 0, true)]
    [DataRow("""(arguments: "arguments", $$fileName: "fileName")""", "arguments", 1, false)]
    public void Constructor_TargetTyped(string constructor, string parameterName, int argumentPosition, bool expected)
    {
        var snippet = $$"""
            System.Diagnostics.ProcessStartInfo psi = new{{constructor}};
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Diagnostics_ProcessStartInfo, parameterName, argumentPosition);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""new Dictionary<TKey, TValue>($$1)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<int, TValue>($$1)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<int, string>($$1)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<TKey, TValue>($$1, EqualityComparer<TKey>.Default)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<TKey, TValue>(1, $$EqualityComparer<TKey>.Default)""", "comparer", 1, true)]
    public void Constructor_Generic(string constructor, string parameterName, int argumentPosition, bool expected)
    {
        var snippet = $$"""
            using System.Collections.Generic;
            class C
            {
                public void M<TKey, TValue>() where TKey : notnull
                {
                    _ = {{constructor}};
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_Dictionary_TKey_TValue, parameterName, argumentPosition);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [TestMethod]
    public void Constructor_BaseCall()
    {
        var snippet = $$"""
            using System.Collections.Generic;
            class MyList: List<int>
            {
                public MyList(int capacity) : base(capacity) // Unsupported
                {
                }
            }
            public class Test
            {
                public void M()
                {
                    _ = new MyList($$1); // Requires tracking of the parameter to the base constructor
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, "capacity", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("""new NumberList($$1)""", "capacity", 0, false)] // FN. Syntactic checks bail out before the semantic model can resolve the alias
    [DataRow("""new($$1)""", "capacity", 0, true)] // Target typed new resolves the alias
    public void Constructor_TypeAlias(string constructor, string parameterName, int argumentPosition, bool expected)
    {
        var snippet = $$"""
            using NumberList = System.Collections.Generic.List<int>;
            class C
            {
                public void M()
                {
                    NumberList nl = {{constructor}};
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, parameterName, argumentPosition);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    private static string WrapInMethod(string snippet) =>
        $$"""
        using System;
        class C
        {
            public void M()
            {
                {{snippet}}
            }
        }
        """;

    private static (SyntaxNode Node, SemanticModel Model) ArgumentAndModel(string snippet)
    {
        var pos = snippet.IndexOf("$$");
        snippet = snippet.Replace("$$", string.Empty);
        var (tree, model) = TestHelper.CompileCS(snippet, MetadataReferenceFacade.SystemCollections.Concat(MetadataReferenceFacade.SystemDiagnosticsProcess).ToArray());
        var node = tree.GetRoot().FindNode(new(pos, 0)).AncestorsAndSelf().First(x => x is ArgumentSyntax or AttributeArgumentSyntax);
        return (node, model);
    }
}
