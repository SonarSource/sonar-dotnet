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

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers.Trackers;

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
    [DataRow("""new($$1)""", "capacity", 0, true)]             // Target typed new resolves the alias
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

    [DataTestMethod]
    [DataRow("""new($$1, 2)""", true)]
    [DataRow("""new C(1, $$2)""", true)]
    [DataRow("""new CAlias(1, $$2)""", true)]
    [DataRow("""new C($$1)""", false)]              // Count constraint fails
    [DataRow("""new C(1, 2, $$3)""", false)]        // Parameter name constraint fails
    [DataRow("""new C($$k: 1, j:2, i:3)""", false)] // Parameter name constraint fails
    public void Constructor_CustomLogic(string constructor, bool expected)
    {
        var snippet = $$"""
            using CAlias = C;
            class C
            {
                public C(int i) { }
                public C(int j, int i) { }
                public C(int j, int i, int k) { }

                public void M()
                {
                    C c = {{constructor}};
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(invokedMethodSymbol: x => x is { MethodKind: MethodKind.Constructor, ContainingSymbol.Name: "C" },
                                                                invokedMemberNameConstraint: (c, n) => c.Equals("C", n) || c.Equals("CAlias"),
                                                                parameterConstraint: p => p.Name is "i" or "j",
                                                                argumentListConstraint: (n, i) => i is null or 0 or 1 && n.Count > 1,
                                                                refKind: null);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [TestMethod]
    public void Constructor_InitializerCalls_This()
    {
        var snippet = $$"""
            class Base
            {
                public Base(int i) : this($$i, 1) { }
                public Base(int i, int j) { }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(invokedMethodSymbol: x => x is { MethodKind: MethodKind.Constructor, ContainingSymbol.Name: "Base" },
                                                                invokedMemberNameConstraint: (c, n) => c.Equals("Base", n),
                                                                parameterConstraint: p => p.Name is "i",
                                                                argumentListConstraint: (_, _) => true,
                                                                refKind: null);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Constructor_InitializerCalls_Base()
    {
        var snippet = $$"""
            class Base
            {
                public Base(int i) { }
            }
            class Derived: Base
            {
                public Derived() : base($$1) { }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(invokedMethodSymbol: x => x is { MethodKind: MethodKind.Constructor, ContainingSymbol.Name: "Base" },
                                                                invokedMemberNameConstraint: (c, n) => c.Equals("Base", n),
                                                                parameterConstraint: p => p.Name is "i",
                                                                argumentListConstraint: (_, _) => true,
                                                                refKind: null);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Indexer_List_Get()
    {
        var snippet = $$"""
            var list = new System.Collections.Generic.List<int>();
            _ = list[$$1];
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T, "list",
            p => p is { Name: "index", Type.SpecialType: SpecialType.System_Int32, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertyGet } }, 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("list[$$1] = 1;")]
    [DataRow("(list[$$1], list[2]) = (1, 2);")]
    [DataRow("list[$$1]++;")]
    [DataRow("list[$$1]--;")]
    public void Indexer_List_Set(string writeExpression)
    {
        var snippet = $$"""
            var list = new System.Collections.Generic.List<int>();
            {{writeExpression}};
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T,
            p => p is { Name: "index", ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertySet } }, 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""Environment.GetEnvironmentVariables()[$$"TEMP"]""")]
    [DataRow("""Environment.GetEnvironmentVariables()?[$$"TEMP"]""")]
    public void Indexer_DictionaryGet(string environmentVariableAccess)
    {
        var snippet = $$"""
            _ = {{environmentVariableAccess}};
            """;
        var (node, model) = ArgumentAndModel(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ElementAccess(m => m is { MethodKind: MethodKind.PropertyGet, ContainingType: { } type } && type.Name == "IDictionary",
            (n, c) => n.Equals("GetEnvironmentVariables", c), p => p.Name == "key", (_, p) => p is null or 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""process.Modules[$$0]""")]
    [DataRow("""process?.Modules[$$0]""")]
    [DataRow("""process.Modules?[$$0]""")]
    [DataRow("""process?.Modules?[$$0]""")]
    [DataRow("""process.Modules[index: $$0]""")]
    [DataRow("""process?.Modules?[index: $$0]""")]
    public void Indexer_ModuleAccess(string modulesAccess)
    {
        var snippet = $$"""
            public class Test
            {
                public void M(System.Diagnostics.Process process)
                {
                    _ = {{modulesAccess}};
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ElementAccess(m => m is { MethodKind: MethodKind.PropertyGet, ContainingType: { } type } && type.Name == "ProcessModuleCollection",
            (n, c) => n.Equals("Modules", c), p => p.Name == "index", (_, p) => p is null or 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""processStartInfo.Environment[$$"TEMP"]""")]
    [DataRow("""processStartInfo?.Environment[$$"TEMP"]""")]
    [DataRow("""processStartInfo.Environment?[$$"TEMP"]""")]
    [DataRow("""processStartInfo?.Environment?[$$"TEMP"]""")]
    [DataRow("""processStartInfo.Environment[key: $$"TEMP"]""")]
    [DataRow("""processStartInfo?.Environment?[key: $$"TEMP"]""")]
    public void Indexer_Environment(string environmentAccess)
    {
        var snippet = $$"""
            public class Test
            {
                public void M(System.Diagnostics.ProcessStartInfo processStartInfo)
                {
                    _ = {{environmentAccess}};
                }
            }
            """;
        var (node, model) = ArgumentAndModel(snippet);

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Environment", p => p.Name == "key", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
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
