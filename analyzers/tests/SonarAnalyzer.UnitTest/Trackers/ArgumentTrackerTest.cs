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

using SonarAnalyzer.Helpers.Trackers;

using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

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
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Method_SimpleArgument_VB()
    {
        var snippet = """
            Dim provider As System.IFormatProvider = Nothing
            Dim i As Integer
            i.ToString($$provider)
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
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
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", position);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""i.ToString($$provider)""", 0, true)]
    [DataRow("""i.ToString($$provider)""", 1, false)]
    [DataRow("""i.ToString("", $$provider)""", 1, true)]
    [DataRow("""i.ToString("", $$provider)""", 0, false)]
    [DataRow("""i.ToString("", $$provider:= provider)""", 1, true)]
    [DataRow("""i.ToString("", $$provider:= provider)""", 0, true)]
    [DataRow("""i.ToString($$provider:= provider, format:= "")""", 1, true)]
    [DataRow("""i.ToString($$provider:= provider, format:= "")""", 0, true)]
    public void Method_Position_VB(string invocation, int position, bool expected)
    {
        var snippet = $$"""
            Dim provider As System.IFormatProvider = Nothing
            Dim i As Integer
            {{invocation}}
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", position);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
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
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", x => true, RefKind.Out);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""Integer.TryParse("", $$result)""")]
    [DataRow("""Integer.TryParse("", System.Globalization.NumberStyles.HexNumber, Nothing, $$result)""")]
    public void Method_RefOut_True_VB(string invocation)
    {
        var snippet = $$"""
            Dim provider As System.IFormatProvider = Nothing
            Dim result As Integer
            {{invocation}}
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", x => true, RefKind.Out);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
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
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

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
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(m => true, (m, c) => m.Equals("M", c), p => p.Name == "parameter", x => true, null);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""Dim a = New Direct().M($$1)""", true)]
    [DataRow("""Dim a = New DirectDifferentParameterName().M($$1)""", false)] // FN. This would require ExplicitOrImplicitInterfaceImplementations from the internal ISymbolExtensions in Roslyn.
    [DataRow("""
        Dim i As I = New Explicit()
        i.M($$1)
        """, true)]
    [DataRow("""
        Dim i As I = New ExplicitDifferentParameterName()
        i.M($$1)
        """, true)]
    public void Method_Inheritance_Interface_VB(string invocation, bool expected)
    {
        var snippet = $$"""
            Interface I
                Function M(ByVal parameter As Integer) As Boolean
            End Interface

            Public Class Direct
                Implements I

                Public Function M(parameter As Integer) As Boolean Implements I.M
                End Function
            End Class

            Public Class DirectDifferentParameterName
                Implements I

                Public Function M(ByVal renamed As Integer) As Boolean Implements I.M
                End Function
            End Class

            Public Class Explicit
                Implements I

                Private Function M(ByVal parameter As Integer) As Boolean Implements I.M
                End Function
            End Class

            Public Class ExplicitDifferentParameterName
                Implements I

                Private Function M(ByVal renamed As Integer) As Boolean Implements I.M
                End Function
            End Class

            Public Class Test
                Private Sub M()
                    {{invocation}}
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(m => true, (m, c) => m.Equals("M", c), p => p.Name == "parameter", x => true, null);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_Generic_Comparer_T, "Compare", "x", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""comparer.Compare($$Nothing, Nothing)""")]
    [DataRow("""Call New MyComparer(Of Integer)().Compare($$1, 2)""")]
    public void Method_Inheritance_BaseClasses_Generics_VB(string invocation)
    {
        var snippet = $$"""
            Imports System.Collections.Generic

            Public Class MyComparer(Of T)
                Inherits Comparer(Of T)

                Public Sub New()
                End Sub

                Public Overrides Function Compare(ByVal a As T, ByVal b As T) As Integer
                    Return 1
                End Function
            End Class

            Public Class Test
                Private Sub M(Of T)(ByVal comparer As MyComparer(Of T))
                    {{invocation}}
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_Generic_Comparer_T, "Compare", "x", 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_CollectionBase, "OnInsert", "index", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""OnInsert($$1, Nothing)""")]
    [DataRow("""OnInsert(position:= $$1, Nothing)""")]
    public void Method_Inheritance_BaseClasses_Overrides_VB(string invocation)
    {
        var snippet = $$"""
            Imports System.Collections

            Public Class Collection(Of T)
                Inherits CollectionBase

                Protected Overrides Sub OnInsert(ByVal position As Integer, ByVal value As Object)
                End Sub

                Private Sub M(ByVal arg As T)
                    {{invocation}}
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_CollectionBase, "OnInsert", "index", 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    // learn.microsoft.com/en-us/dotnet/api/system.string.format
    [DataRow("""string.Format("format", $$0)""", "arg0")]
    [DataRow("""string.Format("format", 0, $$1)""", "arg1")]
    [DataRow("""string.Format("format", 0, 1, $$2)""", "arg2")]
    [DataRow("""string.Format("format", 0, 1, 2, $$3)""", "args")]
    [DataRow("""string.Format("format", arg2: 2, arg1: 1, $$arg0:0)""", "arg0")]
    [DataRow("""string.Format("format", $$new object[0])""", "args")]
    public void Method_ParamsArray(string invocation, string parameterName)
    {
        var snippet = $$"""
                _ = {{invocation}};
            """;
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_String, "Format", parameterName, i => i >= 1);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    // learn.microsoft.com/en-us/dotnet/api/system.string.format
    [DataRow("""String.Format("format", $$0)""", "arg0")]
    [DataRow("""String.Format("format", 0, $$1)""", "arg1")]
    [DataRow("""String.Format("format", 0, 1, $$2)""", "arg2")]
    [DataRow("""String.Format("format", 0, 1, 2, $$3)""", "args")]
    [DataRow("""String.Format("format", arg2:=2, arg1:=1, $$arg0:=0)""", "arg0")]
    [DataRow("""String.Format("format", $$New Object(){ })""", "args")]
    public void Method_ParamsArray_VB(string invocation, string parameterName)
    {
        var snippet = $$"""
                Dim a = {{invocation}}
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.MethodInvocation(KnownType.System_String, "Format", parameterName, i => i >= 1);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""ProcessStartInfo($$"fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo($$"fileName")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 0, false)]
    [DataRow("""ProcessStartInfo($$"fileName", "arguments")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo(arguments:= $$"arguments", fileName:= "fileName")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo(arguments:= $$"arguments", fileName:= "fileName")""", "fileName", 0, false)]
    [DataRow("""ProcessStartInfo(arguments:= "arguments", $$fileName:= "fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo(arguments:= "arguments", $$fileName:= "fileName")""", "arguments", 1, false)]
    public void Constructor_SimpleArgument(string constructor, string parameterName, int argumentPosition, bool expected)
    {
        var snippet = $$"""
            Dim a = New System.Diagnostics.{{constructor}}
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Diagnostics_ProcessStartInfo, parameterName, argumentPosition);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
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
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_Dictionary_TKey_TValue, parameterName, argumentPosition);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""Dim a = new Dictionary(Of TKey, TValue)($$1)""", "capacity", 0, true)]
    [DataRow("""Dim a = new Dictionary(Of Integer, TValue)($$1)""", "capacity", 0, true)]
    [DataRow("""Dim a = new Dictionary(Of Integer, String)($$1)""", "capacity", 0, true)]
    [DataRow("""Dim a = New Dictionary(Of TKey, TValue)($$1, EqualityComparer(Of TKey).Default)""", "capacity", 0, true)]
    [DataRow("""Dim a = new Dictionary(Of TKey, TValue)(1, $$EqualityComparer(Of TKey).Default)""", "comparer", 1, true)]
    public void Constructor_Generic_VB(string constructor, string parameterName, int argumentPosition, bool expected)
    {
        var snippet = $$"""
            Imports System.Collections.Generic

            Class C
                Public Sub M(Of TKey, TValue)()
                    {{constructor}}
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_Dictionary_TKey_TValue, parameterName, argumentPosition);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, "capacity", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeFalse();
    }

    [TestMethod]
    public void Constructor_BaseCall_VB()
    {
        var snippet = $$"""
            Imports System.Collections.Generic

            Class MyList
                Inherits List(Of Integer)

                Public Sub New(ByVal capacity As Integer)
                    MyBase.New(capacity) ' Passing of the parameter to the base constructor is not followed
                End Sub
            End Class

            Public Class Test
                Public Sub M()
                    Dim a = New MyList($$1) ' Requires tracking of the parameter to the base constructor
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, "capacity", 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeFalse();
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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, parameterName, argumentPosition);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [TestMethod]
    public void Constructor_TypeAlias_VB()
    {
        var snippet = $$"""
            Imports NumberList = System.Collections.Generic.List(Of Integer)

            Class C
                Public Sub M()
                    Dim nl As NumberList = New NumberList($$1)
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, "capacity", 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeFalse("FN. Syntactic check does not respect aliases.");
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
        var (node, model) = ArgumentAndModelCS(snippet);

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
        var (node, model) = ArgumentAndModelCS(snippet);

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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(invokedMethodSymbol: x => x is { MethodKind: MethodKind.Constructor, ContainingSymbol.Name: "Base" },
                                                                invokedMemberNameConstraint: (c, n) => c.Equals("Base", n),
                                                                parameterConstraint: p => p.Name is "i",
                                                                argumentListConstraint: (_, _) => true,
                                                                refKind: null);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Constructor_InitializerCalls_Base_MyException()
    {
        var snippet = """
            using System;

            class MyException: Exception
            {
                public MyException(string message) : base($$message)
                { }
            }
            """;
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Exception, "message", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Constructor_InitializerCalls_Base_MyException_VB()
    {
        var snippet = $$"""
            Imports System

            Public Class MyException
                Inherits Exception

                Public Sub New(ByVal message As String)
                    MyBase.New($$message)
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Exception, "message", 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeFalse("FN. MyBase.New and Me.New are not supported.");
    }

    [TestMethod]
    public void Indexer_List_Get()
    {
        var snippet = $$"""
            var list = new System.Collections.Generic.List<int>();
            _ = list[$$1];
            """;
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T, "list",
            p => p is { Name: "index", Type.SpecialType: SpecialType.System_Int32, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertyGet } }, 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Indexer_List_Get_VB()
    {
        var snippet = $$"""
            Dim list = New System.Collections.Generic.List(Of Integer)()
            Dim a = list($$1)
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T, "list",
            p => p is { Name: "index", Type.SpecialType: SpecialType.System_Int32, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertyGet } }, 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
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
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T,
            p => p is { Name: "index", ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertySet } }, 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("list($$1) = 1")]
    [DataRow("list($$1) += 1")]
    [DataRow("list($$1) -= 1")]
    public void Indexer_List_Set_VB(string writeExpression)
    {
        var snippet = $$"""
            Dim list = New System.Collections.Generic.List(Of Integer)()
            {{writeExpression}}
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T,
            p => p is { Name: "index", ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertySet } }, 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""Environment.GetEnvironmentVariables()[$$"TEMP"]""")]
    [DataRow("""Environment.GetEnvironmentVariables()?[$$"TEMP"]""")]
    public void Indexer_DictionaryGet(string environmentVariableAccess)
    {
        var snippet = $$"""
            _ = {{environmentVariableAccess}};
            """;
        var (node, model) = ArgumentAndModelCS(WrapInMethod(snippet));

        var argument = ArgumentDescriptor.ElementAccess(m => m is { MethodKind: MethodKind.PropertyGet, ContainingType: { } type } && type.Name == "IDictionary",
            (n, c) => n.Equals("GetEnvironmentVariables", c), p => p.Name == "key", (_, p) => p is null or 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Indexer_DictionaryGet_VB()
    {
        var snippet = """
            Dim a = Environment.GetEnvironmentVariables()($$"TEMP")
            """;
        var (node, model) = ArgumentAndModelVB(WrapInMethodVB(snippet));

        var argument = ArgumentDescriptor.ElementAccess(m => m is { MethodKind: MethodKind.PropertyGet, ContainingType: { } type } && type.Name == "IDictionary",
            (n, c) => n.Equals("GetEnvironmentVariables", c), p => p.Name == "key", (_, p) => p is null or 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""process.Modules[$$0]""")]
    [DataRow("""process?.Modules[$$0]""")]
    [DataRow("""process.Modules?[$$0]""")]
    [DataRow("""process?.Modules?[$$0]""")]
    [DataRow("""process.Modules[index: $$0]""")]
    [DataRow("""process?.Modules?[index: $$0]""")]
    public void Indexer_ModulesAccess(string modulesAccess)
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
        var (node, model) = ArgumentAndModelCS(snippet);

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
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Environment", p => p.Name == "key", 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Attribute_Obsolete()
    {
        var snippet = $$"""
            using System;
            public class Test
            {
                [Obsolete($$"message", UrlFormat = "")]
                public void M()
                {
                }
            }
            """;
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.AttributeArgument(x => x is { MethodKind: MethodKind.Constructor, ContainingType.Name: "ObsoleteAttribute" },
            (s, c) => s.StartsWith("Obsolete", c), p => p.Name == "message", (_, i) => i is 0);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [TestMethod]
    public void Attribute_Obsolete_VB()
    {
        var snippet = $$"""
            Imports System

            Public Class Test
                <Obsolete($$"message", UrlFormat:="")>
                Public Sub M()
                End Sub
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.AttributeArgument(x => x is { MethodKind: MethodKind.Constructor, ContainingType.Name: "ObsoleteAttribute" },
            (s, c) => s.StartsWith("Obsolete", c), p => p.Name == "message", (_, i) => i is 0);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""[Designer($$"designerTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[DesignerAttribute($$"designerTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[DesignerAttribute($$"designerTypeName", "designerBaseTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[DesignerAttribute("designerTypeName", $$"designerBaseTypeName")]""", "designerBaseTypeName", 1)]
    [DataRow("""[Designer($$designerBaseTypeName: "designerBaseTypeName", designerTypeName: "designerTypeName")]""", "designerBaseTypeName", 1)]
    [DataRow("""[Designer(designerBaseTypeName: "designerBaseTypeName", $$designerTypeName: "designerTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[Designer(designerBaseTypeName: "designerBaseTypeName", $$designerTypeName: "designerTypeName")]""", "designerTypeName", 1)]
    public void Attribute_Designer(string attribute, string parameterName, int argumentPosition)
    {
        var snippet = $$"""
            using System.ComponentModel;

            {{attribute}}
            public class Test
            {
                public void M()
                {
                }
            }
            """;
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.AttributeArgument("Designer", parameterName, argumentPosition);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""<Designer($$"designerTypeName")>""", "designerTypeName", 0)]
    [DataRow("""<DesignerAttribute($$"designerTypeName")>""", "designerTypeName", 0)]
    [DataRow("""<DesignerAttribute($$"designerTypeName", "designerBaseTypeName")>""", "designerTypeName", 0)]
    [DataRow("""<DesignerAttribute("designerTypeName", $$"designerBaseTypeName")>""", "designerBaseTypeName", 1)]
    public void Attribute_Designer_VB(string attribute, string parameterName, int argumentPosition)
    {
        var snippet = $$"""
            Imports System.ComponentModel

            {{attribute}}
            Public Class Test
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.AttributeArgument("Designer", parameterName, argumentPosition);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true)]""", "AllowMultiple", true)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true, Inherited = true)]""", "AllowMultiple", true)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true, Inherited = true)]""", "Inherited", false)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  AllowMultiple = true, $$Inherited = true)]""", "Inherited", true)]
    public void Attribute_Property(string attribute, string propertyName, bool expected)
    {
        var snippet = $$"""
            using System;

            {{attribute}}
            public sealed class TestAttribute: Attribute
            {
            }
            """;
        var (node, model) = ArgumentAndModelCS(snippet);

        var argument = ArgumentDescriptor.AttributeProperty("AttributeUsage", propertyName);
        new CSharpArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
    }

    [DataTestMethod]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  $$AllowMultiple := true)>""", "AllowMultiple", true)]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  $$AllowMultiple := true, Inherited := true)>""", "AllowMultiple", true)]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  $$AllowMultiple := true, Inherited := true)>""", "Inherited", false)]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  AllowMultiple := true, $$Inherited := true)>""", "Inherited", true)]
    public void Attribute_Property_VB(string attribute, string propertyName, bool expected)
    {
        var snippet = $$"""
            Imports System

            {{attribute}}
            Public NotInheritable Class TestAttribute
                Inherits Attribute
            End Class
            """;
        var (node, model) = ArgumentAndModelVB(snippet);

        var argument = ArgumentDescriptor.AttributeProperty("AttributeUsage", propertyName);
        new VisualBasicArgumentTracker().MatchArgument(argument)(new SyntaxBaseContext(node, model)).Should().Be(expected);
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

    private static string WrapInMethodVB(string snippet) =>
        $$"""
        Imports System
        Class C
            Public Sub M()
                {{snippet}}
            End Sub
        End Class
        """;

    private static (SyntaxNode Node, SemanticModel Model) ArgumentAndModel(string snippet, Func<string, MetadataReference[], (SyntaxTree, SemanticModel)> compile, params Type[] argumentNodeTypes)
    {
        var pos = snippet.IndexOf("$$");
        snippet = snippet.Replace("$$", string.Empty);
        var (tree, model) = compile(snippet, MetadataReferenceFacade.SystemCollections.Concat(MetadataReferenceFacade.SystemDiagnosticsProcess).ToArray());
        var node = tree.GetRoot().FindNode(new(pos, 0)).AncestorsAndSelf().First(x => argumentNodeTypes.Any(t => t.IsAssignableFrom(x.GetType())));
        return (node, model);
    }

    private static (SyntaxNode Node, SemanticModel Model) ArgumentAndModelCS(string snippet) =>
        ArgumentAndModel(snippet, TestHelper.CompileCS, typeof(CS.ArgumentSyntax), typeof(CS.AttributeArgumentSyntax));

    private static (SyntaxNode Node, SemanticModel Model) ArgumentAndModelVB(string snippet) =>
        ArgumentAndModel(snippet, TestHelper.CompileVB, typeof(VB.ArgumentSyntax));
}
