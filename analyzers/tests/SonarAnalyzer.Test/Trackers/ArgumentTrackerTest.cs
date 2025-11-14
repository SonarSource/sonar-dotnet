/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Core.Semantics.Extensions;
using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.CSharp.Core.Trackers;
using SonarAnalyzer.VisualBasic.Core.Trackers;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Trackers;

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
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.Name.Should().Be("provider");
        context.Parameter.Type.Name.Should().Be("IFormatProvider");
    }

    [TestMethod]
    public void Method_SimpleArgument_VB()
    {
        var snippet = """
            Dim provider As System.IFormatProvider = Nothing
            Dim i As Integer
            i.ToString($$provider)
            """;
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.Name.Should().Be("provider");
        context.Parameter.Type.Name.Should().Be("IFormatProvider");
    }

    [TestMethod]
    [DataRow("""M( $$ ,  , 1)""", "i", true)]
    [DataRow("""M( $$ ,  , 1)""", "j", false)]
    [DataRow("""M(  , $$ , 1)""", "j", true)]
    [DataRow("""M(  ,  , $$1)""", "k", true)]
    [DataRow("""M(  ,  , $$1)""", "i", false)]
    public void Method_OmittedArgument_VB(string invocation, string parameterName, bool expected)
    {
        var snippet = $$"""
            Public Class C
                Public Sub M(Optional i As Integer = 0, Optional j As Integer = 0, Optional k As Integer = 0)
                    {{invocation}}
                End Sub
            End Class
            """;
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(x => x.Name == "M", (s, c) => s.Equals("M", c), x => x.Name == parameterName, _ => true, null);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
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
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", position);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
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
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "ToString", "provider", position);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("""int.TryParse("", $$out var result);""")]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""")]
    public void Method_RefOut_True(string invocation)
    {
        var snippet = $$"""
            System.IFormatProvider provider = null;
            {{invocation}}
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", _ => true, RefKind.Out);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.RefKind.Should().Be(RefKind.Out);
    }

    [TestMethod]
    [DataRow("""Integer.TryParse("", $$result)""")]
    [DataRow("""Integer.TryParse("", System.Globalization.NumberStyles.HexNumber, Nothing, $$result)""")]
    public void Method_RefOut_True_VB(string invocation)
    {
        var snippet = $$"""
            Dim provider As System.IFormatProvider = Nothing
            Dim result As Integer
            {{invocation}}
            """;
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", _ => true, RefKind.Out);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""int.TryParse("", $$out var result);""", RefKind.Ref)]
    [DataRow("""int.TryParse("", $$out var result);""", RefKind.In)]
    [DataRow("""int.TryParse("", $$out var result);""", RefKind.RefReadOnlyParameter)]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""", RefKind.Ref)]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""", RefKind.In)]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""", RefKind.RefReadOnlyParameter)]
    [DataRow("""int.TryParse($$"", out var result);""", RefKind.Out)]
    [DataRow("""int.TryParse("", $$System.Globalization.NumberStyles.HexNumber, null, out var result);""", RefKind.Out)]
    public void Method_RefOut_False(string invocation, RefKind refKind)
    {
        var snippet = $$"""
            System.IFormatProvider provider = null;
            {{invocation}}
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", _ => true, refKind);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeFalse();
        context.Parameter.Should().BeNull();
    }

    [TestMethod]
    [DataRow("""int.TryParse("", $$out var result);""")]
    [DataRow("""int.TryParse("", System.Globalization.NumberStyles.HexNumber, null, $$out var result);""")]
    public void Method_RefOut_Unspecified(string invocation)
    {
        var snippet = $$"""
            System.IFormatProvider provider = null;
            {{invocation}}
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));
        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Int32, "TryParse", "result", _ => true);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.RefKind.Should().Be(RefKind.Out);
    }

    [TestMethod]
    [DataRow("in s")]
    [DataRow("s")] // "in" is optional on the call side
    [DataRow("ref s")] // Valid, but produces warning CS9191: The 'ref' modifier for argument 1 corresponding to 'in' parameter is equivalent to 'in'. Consider using 'in' instead.
    public void Method_RefIn(string argument)
    {
        var snippet = $$"""
            public readonly struct S { public readonly int I; }

            public class C
            {
                public void M(in S s) { }

                public void Test()
                {
                    var s = new S();
                    M($${{argument}});
                }
            }
            """;
        var context = ArgumentContextCS(snippet);
        var descriptor = ArgumentDescriptor.MethodInvocation(_ => true, (x, c) => string.Equals(x, "M", c), _ => true, _ => true, RefKind.In);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.RefKind.Should().Be(RefKind.In);
    }

    [TestMethod]
    [DataRow("in s")]
    [DataRow("s")] // Valid, produces warning CS9192: Argument 1 should be passed with 'ref' or 'in' keyword
    [DataRow("ref s")]
    public void Method_RefReadOnly(string argument)
    {
        var snippet = $$"""
            public readonly struct S { public readonly int I; }

            public class C
            {
                public void M(ref readonly S s) { }

                public void Test()
                {
                    var s = new S();
                    M($${{argument}});
                }
            }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(_ => true, (x, c) => string.Equals(x, "M", c), _ => true, _ => true, RefKind.RefReadOnlyParameter);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.RefKind.Should().Be(RefKind.RefReadOnlyParameter);
    }

    [TestMethod]
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(_ => true, (m, c) => m.Equals("M", c), x => x.Name == "parameter", _ => true, null);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
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
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(_ => true, (m, c) => m.Equals("M", c), x => x.Name == "parameter", _ => true, null);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("""comparer.Compare($$default, default);""")]
    [DataRow("""new MyComparer<int>().Compare($$1, 2);""")]
    public void Method_Inheritance_BaseClasses_Generics(string invocation)
    {
        var snippet = $$"""
            using System.Collections.Generic;
            public class MyComparer<T> : Comparer<T>
            {
                public MyComparer() { }
                public override int Compare(T a, T b) => 1; // The original definition uses x and y: int Compare(T? x, T? y)
            }
            public class Test
            {
                void M<T>(MyComparer<T> comparer)
                {
                    {{invocation}}
                }
            }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_Generic_Comparer_T, "Compare", "x", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""comparer.Compare($$Nothing, Nothing)""", "x", true)]
    [DataRow("""comparer.Compare($$Nothing, Nothing)""", "a", false)]
    [DataRow("""Call New MyComparer(Of Integer)().Compare($$1, 2)""", "x", true)]
    [DataRow("""Call New MyComparer(Of Integer)().Compare($$1, 2)""", "a", false)]
    public void Method_Inheritance_BaseClasses_Generics_VB(string invocation, string parameterName, bool expected)
    {
        var snippet = $$"""
            Imports System.Collections.Generic

            Public Class MyComparer(Of T)
                Inherits Comparer(Of T)

                Public Overrides Function Compare(ByVal a As T, ByVal b As T) As Integer ' The original definition uses x and y
                    Return 1
                End Function
            End Class

            Public Class Test
                Private Sub M(Of T)(ByVal comparer As MyComparer(Of T))
                    {{invocation}}
                End Sub
            End Class
            """;
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_Generic_Comparer_T, "Compare", parameterName, 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_CollectionBase, "OnInsert", "index", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
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
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Collections_CollectionBase, "OnInsert", "index", 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    // learn.microsoft.com/en-us/dotnet/api/system.string.format
    [DataRow("""string.Format("format", $$0)""", "arg0")]
    [DataRow("""string.Format("format", 0, $$1)""", "arg1")]
    [DataRow("""string.Format("format", 0, 1, $$2)""", "arg2")]
    [DataRow("""string.Format("format", 0, 1, 2, $$3)""", "args")]
    [DataRow("""string.Format("format", 0, 1, $$2, 3)""", "args")]
    [DataRow("""string.Format("format", 0, $$1, 2, 3)""", "args")]
    [DataRow("""string.Format("format", $$0, 1, 2, 3)""", "args")]
    [DataRow("""string.Format("format", arg2: 2, arg1: 1, $$arg0:0)""", "arg0")]
    [DataRow("""string.Format("format", $$new object[0])""", "args")]
    public void Method_ParamsArray(string invocation, string parameterName)
    {
        var snippet = $$"""
                _ = {{invocation}};
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_String, "Format", parameterName, x => x >= 1);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
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
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_String, "Format", parameterName, x => x >= 1);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Method_NamelessMethod()
    {
        var snippet = """
                using System;
                class C
                {
                    Action<int> ActionReturning() => null;

                    void M()
                    {
                        ActionReturning()($$1);
                    }
                }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(KnownType.System_Action_T, methodName: string.Empty, "obj", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.Name.Should().Be("obj");
        context.Parameter.ContainingSymbol.Name.Should().Be("Invoke");
        context.Parameter.ContainingType.Name.Should().Be("Action");
    }

    [TestMethod]
    public void Method_InvocationOnProperty()
    {
        var snippet = """
                using System.Collections.Generic;
                class C
                {
                    public IList<int> List { get; } = new List<int>();
                    void M()
                    {
                        List.Add($$1); // Add is defined on ICollection<T> while the List property is of type IList<T>, invokedMemberNodeConstraint can figure this out
                    }
                }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.MethodInvocation(
            invokedMemberConstraint: x => x.Is(KnownType.System_Collections_Generic_ICollection_T, "Add"),
            invokedMemberNameConstraint: (x, c) => string.Equals(x, "Add", c),
            invokedMemberNodeConstraint: (model, language, node) =>
                node is CS.InvocationExpressionSyntax { Expression: CS.MemberAccessExpressionSyntax { Expression: CS.IdentifierNameSyntax { Identifier.ValueText: { } leftName } left } }
                    && language.NameComparer.Equals(leftName, "List")
                    && model.GetSymbolInfo(left).Symbol is IPropertySymbol property
                    && property.Type.Is(KnownType.System_Collections_Generic_IList_T),
            parameterConstraint: _ => true,
            argumentListConstraint: (_, _) => true,
            refKind: null);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.Name.Should().Be("item");
        context.Parameter.ContainingSymbol.Name.Should().Be("Add");
        context.Parameter.ContainingType.Name.Should().Be("ICollection");
    }

    [TestMethod]
    [DataRow("""ProcessStartInfo($$"fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo($$"fileName")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 0, false)]
    [DataRow("""ProcessStartInfo($$"fileName", "arguments")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo(arguments: $$"arguments", fileName: "fileName")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo(arguments: $$"arguments", fileName: "fileName")""", "fileName", 0, false)]
    [DataRow("""ProcessStartInfo(arguments: "arguments", $$fileName: "fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo(arguments: "arguments", $$fileName: "fileName")""", "arguments", 1, false)]
    public void Constructor_SimpleArgument(string constructor, string parameterName, int descriptorPosition, bool expected)
    {
        var snippet = $$"""
            _ = new System.Diagnostics.{{constructor}};
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Diagnostics_ProcessStartInfo, parameterName, descriptorPosition);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
        if (expected)
        {
            context.Parameter.Name.Should().Be(parameterName);
            context.Parameter.ContainingSymbol.Name.Should().Be(".ctor");
            context.Parameter.ContainingType.Name.Should().Be("ProcessStartInfo");
        }
    }

    [TestMethod]
    [DataRow("""ProcessStartInfo($$"fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo($$"fileName")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo("fileName", $$"arguments")""", "arguments", 0, false)]
    [DataRow("""ProcessStartInfo($$"fileName", "arguments")""", "arguments", 1, false)]
    [DataRow("""ProcessStartInfo(arguments:= $$"arguments", fileName:= "fileName")""", "arguments", 1, true)]
    [DataRow("""ProcessStartInfo(arguments:= $$"arguments", fileName:= "fileName")""", "fileName", 0, false)]
    [DataRow("""ProcessStartInfo(arguments:= "arguments", $$fileName:= "fileName")""", "fileName", 0, true)]
    [DataRow("""ProcessStartInfo(arguments:= "arguments", $$fileName:= "fileName")""", "arguments" +
        "", 1, false)]
    public void Constructor_SimpleArgument_VB(string constructor, string parameterName, int descriptorPosition, bool expected)
    {
        var snippet = $$"""
            Dim a = New System.Diagnostics.{{constructor}}
            """;
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Diagnostics_ProcessStartInfo, parameterName, descriptorPosition);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("""($$"fileName")""", "fileName", 0, true)]
    [DataRow("""($$"fileName")""", "arguments", 1, false)]
    [DataRow("""("fileName", $$"arguments")""", "arguments", 1, true)]
    [DataRow("""("fileName", $$"arguments")""", "arguments", 0, false)]
    [DataRow("""($$"fileName", "arguments")""", "arguments", 1, false)]
    [DataRow("""(arguments: $$"arguments", fileName: "fileName")""", "arguments", 1, true)]
    [DataRow("""(arguments: $$"arguments", fileName: "fileName")""", "fileName", 0, false)]
    [DataRow("""(arguments: "arguments", $$fileName: "fileName")""", "fileName", 0, true)]
    [DataRow("""(arguments: "arguments", $$fileName: "fileName")""", "arguments", 1, false)]
    public void Constructor_TargetTyped(string constructor, string parameterName, int descriptorPosition, bool expected)
    {
        var snippet = $$"""
            System.Diagnostics.ProcessStartInfo psi = new{{constructor}};
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Diagnostics_ProcessStartInfo, parameterName, descriptorPosition);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("""new Dictionary<TKey, TValue>($$1)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<int, TValue>($$1)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<int, string>($$1)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<TKey, TValue>($$1)""", "comparer", 0, false)]
    [DataRow("""new Dictionary<TKey, TValue>($$1, EqualityComparer<TKey>.Default)""", "capacity", 0, true)]
    [DataRow("""new Dictionary<TKey, TValue>(1, $$EqualityComparer<TKey>.Default)""", "comparer", 1, true)]
    public void Constructor_Generic(string constructor, string parameterName, int descriptorPosition, bool expected)
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_Dictionary_TKey_TValue, parameterName, descriptorPosition);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("""Dim a = new Dictionary(Of TKey, TValue)($$1)""", "capacity", 0, true)]
    [DataRow("""Dim a = new Dictionary(Of Integer, TValue)($$1)""", "capacity", 0, true)]
    [DataRow("""Dim a = new Dictionary(Of Integer, String)($$1)""", "capacity", 0, true)]
    [DataRow("""Dim a = new Dictionary(Of TKey, TValue)($$1)""", "comparer", 0, false)]
    [DataRow("""Dim a = New Dictionary(Of TKey, TValue)($$1, EqualityComparer(Of TKey).Default)""", "capacity", 0, true)]
    [DataRow("""Dim a = new Dictionary(Of TKey, TValue)(1, $$EqualityComparer(Of TKey).Default)""", "comparer", 1, true)]
    public void Constructor_Generic_VB(string constructor, string parameterName, int descriptorPosition, bool expected)
    {
        var snippet = $$"""
            Imports System.Collections.Generic

            Class C
                Public Sub M(Of TKey, TValue)()
                    {{constructor}}
                End Sub
            End Class
            """;
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_Dictionary_TKey_TValue, parameterName, descriptorPosition);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().Be(expected);
    }

    [TestMethod]
    public void Constructor_BaseCall()
    {
        var snippet = $$"""
            using System.Collections.Generic;
            class MyList: List<int>
            {
                public MyList(int capacity) : base(capacity) // Forwarded constructor parameter are unsupported
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, "capacity", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeFalse();
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
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, "capacity", 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("""new NumberList($$1)""", "capacity", 0, false)] // FN. Syntactic checks bail out before the semantic model can resolve the alias
    [DataRow("""new($$1)""", "capacity", 0, true)]             // Target typed new resolves the alias
    public void Constructor_TypeAlias(string constructor, string parameterName, int descriptorPosition, bool expected)
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, parameterName, descriptorPosition);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
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
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Collections_Generic_List_T, "capacity", 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeFalse("FN. Syntactic check does not respect aliases.");
    }

    [TestMethod]
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(invokedMethodSymbol: x => x is { MethodKind: MethodKind.Constructor, ContainingSymbol.Name: "C" },
                                                                  invokedMemberNameConstraint: (c, n) => c.Equals("C", n) || c.Equals("CAlias"),
                                                                  invokedMemberNodeConstraint: (_, _, _) => true,
                                                                  parameterConstraint: x => x.Name is "i" or "j",
                                                                  argumentListConstraint: (n, i) => i is null or 0 or 1 && n.Count > 1,
                                                                  refKind: null);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(invokedMethodSymbol: x => x is { MethodKind: MethodKind.Constructor, ContainingSymbol.Name: "Base" },
                                                                  invokedMemberNameConstraint: (c, n) => c.Equals("Base", n),
                                                                  invokedMemberNodeConstraint: (_, _, _) => true,
                                                                  parameterConstraint: x => x.Name is "i",
                                                                  argumentListConstraint: (_, _) => true,
                                                                  refKind: null);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(invokedMethodSymbol: x => x is { MethodKind: MethodKind.Constructor, ContainingSymbol.Name: "Base" },
                                                                  invokedMemberNameConstraint: (c, n) => c.Equals("Base", n),
                                                                  invokedMemberNodeConstraint: (_, _, _) => true,
                                                                  parameterConstraint: x => x.Name is "i",
                                                                  argumentListConstraint: (_, _) => true,
                                                                  refKind: null);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Exception, "message", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
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
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.ConstructorInvocation(KnownType.System_Exception, "message", 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeFalse("FN. MyBase.New and Me.New are not supported.");
    }

    [TestMethod]
    public void Indexer_List_Get()
    {
        var snippet = $$"""
            var list = new System.Collections.Generic.List<int>();
            _ = list[$$1];
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T, "list",
            x => x is { Name: "index", Type.SpecialType: SpecialType.System_Int32, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertyGet } }, 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.Name.Should().Be("index");
        var associatedSymbol = context.Parameter.ContainingSymbol.Should().BeAssignableTo<IMethodSymbol>().Which.AssociatedSymbol.Should().BeAssignableTo<IPropertySymbol>().Which;
        associatedSymbol.IsIndexer.Should().BeTrue();
        associatedSymbol.Name.Should().Be("this[]");
    }

    [TestMethod]
    public void Indexer_List_Get_VB()
    {
        var snippet = $$"""
            Dim list = New System.Collections.Generic.List(Of Integer)()
            Dim a = list($$1)
            """;
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T, "list",
            x => x is { Name: "index", Type.SpecialType: SpecialType.System_Int32, ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertyGet } }, 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.Name.Should().Be("index");
        var associatedSymbol = context.Parameter.ContainingSymbol.Should().BeAssignableTo<IMethodSymbol>().Which.AssociatedSymbol.Should().BeAssignableTo<IPropertySymbol>().Which;
        associatedSymbol.IsIndexer.Should().BeTrue();
        associatedSymbol.Name.Should().Be("Item");
    }

    [TestMethod]
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
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T,
            x => x is { Name: "index", ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertySet } }, 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.ContainingSymbol.Should().BeAssignableTo<IMethodSymbol>().Which.MethodKind.Should().Be(MethodKind.PropertySet);
    }

    [TestMethod]
    [DataRow("list($$1) = 1")]
    [DataRow("list($$1) += 1")]
    [DataRow("list($$1) -= 1")]
    public void Indexer_List_Set_VB(string writeExpression)
    {
        var snippet = $$"""
            Dim list = New System.Collections.Generic.List(Of Integer)()
            {{writeExpression}}
            """;
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_List_T,
            x => x is { Name: "index", ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.PropertySet } }, 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
        context.Parameter.ContainingSymbol.Should().BeAssignableTo<IMethodSymbol>().Which.MethodKind.Should().Be(MethodKind.PropertySet);
    }

    [TestMethod]
    [DataRow("""Environment.GetEnvironmentVariables()[$$"TEMP"]""")]
    [DataRow("""Environment.GetEnvironmentVariables()?[$$"TEMP"]""")]
    public void Indexer_DictionaryGet(string environmentVariableAccess)
    {
        var snippet = $$"""
            _ = {{environmentVariableAccess}};
            """;
        var context = ArgumentContextCS(WrapInMethodCS(snippet));

        var descriptor = ArgumentDescriptor.ElementAccess(x => x is { MethodKind: MethodKind.PropertyGet, ContainingType.Name: "IDictionary" },
            (n, c) => n.Equals("GetEnvironmentVariables", c), (_, _, _) => true, x => x.Name == "key", (_, p) => p is null or 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Indexer_DictionaryGet_VB()
    {
        var snippet = """
            Dim a = Environment.GetEnvironmentVariables()($$"TEMP")
            """;
        var context = ArgumentContextVB(WrapInMethodVB(snippet));

        var descriptor = ArgumentDescriptor.ElementAccess(x => x is { MethodKind: MethodKind.PropertyGet, ContainingType.Name: "IDictionary" },
            (n, c) => n.Equals("GetEnvironmentVariables", c), (_, _, _) => true, x => x.Name == "key", (_, p) => p is null or 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""_ = this[$$0,0];""", "x", true)]
    [DataRow("""_ = this[0,$$0];""", "y", true)]
    [DataRow("""_ = this[$$y: 0,x: 0];""", "y", true)]
    [DataRow("""_ = this[y: 0,$$x: 0];""", "x", true)]
    [DataRow("""this[$$0, 0] = 1;""", "x", false)]
    [DataRow("""this[0, $$0] = 1;""", "y", false)]
    [DataRow("""this[y: $$0, x: 0] = 1;""", "y", false)]
    [DataRow("""this[y: 0, $$x: 0] = 1;""", "x", false)]
    public void Indexer_MultiDimensional(string access, string parameterName, bool isGetter)
    {
        var snippet = $$"""
            public class C {
                public int this[int x, int y]
                {
                    get => 1;
                    set { }
                }

                public void M() {
                    {{access}}
                }
            }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ElementAccess(
            x => x is { MethodKind: var kind, ContainingType.Name: "C" } && (isGetter ? kind == MethodKind.PropertyGet : kind == MethodKind.PropertySet),
            (_, _) => true,
            (_, _, _) => true,
            x => x.Name == parameterName,
            (_, _) => true);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""Dim a = Me($$0, 0)""", "x", true)]
    [DataRow("""Dim a = Me(0, $$0)""", "y", true)]
    [DataRow("""Dim a = Me(y := $$0, x := 0)""", "y", true)]
    [DataRow("""Dim a = Me(y := 0, $$x := 0)""", "x", true)]
    [DataRow("""Me($$0, 0) = 1""", "x", false)]
    [DataRow("""Me(0, $$0) = 1""", "y", false)]
    [DataRow("""Me(y := $$0, x := 0) = 1""", "y", false)]
    [DataRow("""Me(y := 0, $$x := 0) = 1""", "x", false)]
    public void Indexer_MultiDimensional_VB(string access, string parameterName, bool isGetter)
    {
        var snippet = $$"""
            Public Class C
                Default Public Property Item(ByVal x As Integer, ByVal y As Integer) As Integer
                    Get
                        Return 1
                    End Get
                    Set(ByVal value As Integer)
                    End Set
                End Property

                Public Sub M()
                    {{access}}
                End Sub
            End Class
            """;
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.ElementAccess(
            x => x is { MethodKind: var kind, ContainingType.Name: "C" } && (isGetter ? kind == MethodKind.PropertyGet : kind == MethodKind.PropertySet),
            (_, _) => true,
            (_, _, _) => true,
            x => x.Name == parameterName,
            (_, _) => true);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""process.Modules[$$0]""")]
    [DataRow("""process?.Modules[$$0]""")]
    [DataRow("""process.Modules?[$$0]""")]
    [DataRow("""process?.Modules?[$$0]""")]
    [DataRow("""process.Modules[index: $$0]""")]
    [DataRow("""process?.Modules?[index: $$0]""")]
    public void Indexer_ConditionalAccess_Collection(string modulesAccess)
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ElementAccess(x => x is { MethodKind: MethodKind.PropertyGet, ContainingType.Name: "ProcessModuleCollection" },
            (n, c) => n.Equals("Modules", c),
            (_, _, _) => true,
            x => x.Name == "index",
            (_, p) => p is null or 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""processStartInfo.Environment[$$"TEMP"]""")]
    [DataRow("""processStartInfo?.Environment[$$"TEMP"]""")]
    [DataRow("""processStartInfo.Environment?[$$"TEMP"]""")]
    [DataRow("""processStartInfo?.Environment?[$$"TEMP"]""")]
    [DataRow("""processStartInfo.Environment[key: $$"TEMP"]""")]
    [DataRow("""processStartInfo?.Environment?[key: $$"TEMP"]""")]
    public void Indexer_ConditionalAccess_Dictionary(string environmentAccess)
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.ElementAccess(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, "Environment", x => x.Name == "key", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""this[$$"TEMP"]""", true)]
    [DataRow("""this[42, $$"TEMP"]""", false)]
    [DataRow("""this[42, 43, $$"TEMP"]""", true)]
    public void Indexer_ConditionalPosition(string expression, bool expectedResult)
    {
        var snippet = $$"""
            public class Test
            {
                public int this[string key] => 0;
                public int this[int a, string key] => 0;
                public int this[int a, int b, string key] => 0;

                public void M()
                {
                    _ = {{expression}};
                }
            }
            """;
        var context = ArgumentContextCS(snippet);
        var descriptor = ArgumentDescriptor.ElementAccess(
            new KnownType("Test"),
            x => x.Name == "key",
            x => x is 0 or 2);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expectedResult);
    }

    [TestMethod]
    [DataRow("System.Int32", false)]
    [DataRow("System.Collections.Generic.IDictionary", true)]
    public void Indexer_WrongKnownType(string type, bool expectedResult)
    {
        var snippet = $$"""
            public class Test
            {
                public void M(System.Diagnostics.ProcessStartInfo psi)
                {
                    _ = psi.Environment?[key: $$"TEMP"];
                }
            }
            """;
        var context = ArgumentContextCS(snippet);
        var descriptor = ArgumentDescriptor.ElementAccess(
            new(type, "TKey", "TValue"),
            "Environment",
            x => x.Name == "key",
            0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expectedResult);
    }

#if NET

    [TestMethod]
    public void Attribute_ConstructorParameter()
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.AttributeArgument(x => x is { MethodKind: MethodKind.Constructor, ContainingType.Name: "ObsoleteAttribute" },
            (s, c) => s.StartsWith("Obsolete", c),
            (_, _, _) => true,
            x => x.Name == "message",
            (_, i) => i is 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Attribute_ConstructorParameter_VB()
    {
        var snippet = $$"""
            Imports System

            Public Class Test
                <Obsolete($$"message", UrlFormat:="")>
                Public Sub M()
                End Sub
            End Class
            """;
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.AttributeArgument(x => x is { MethodKind: MethodKind.Constructor, ContainingType.Name: "ObsoleteAttribute" },
            (s, c) => s.StartsWith("Obsolete", c),
            (_, _, _) => true,
            x => x.Name == "message",
            (_, i) => i is 0);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
    }

#endif

    [TestMethod]
    [DataRow("""[Designer($$"designerTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[DesignerAttribute($$"designerTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[DesignerAttribute($$"designerTypeName", "designerBaseTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[DesignerAttribute("designerTypeName", $$"designerBaseTypeName")]""", "designerBaseTypeName", 1)]
    [DataRow("""[Designer($$designerBaseTypeName: "designerBaseTypeName", designerTypeName: "designerTypeName")]""", "designerBaseTypeName", 1)]
    [DataRow("""[Designer(designerBaseTypeName: "designerBaseTypeName", $$designerTypeName: "designerTypeName")]""", "designerTypeName", 0)]
    [DataRow("""[Designer(designerBaseTypeName: "designerBaseTypeName", $$designerTypeName: "designerTypeName")]""", "designerTypeName", 1)]
    [DataRow("""[Designer($$"designerTypeName"$$, "designerBaseTypeName"), DesignerCategory("Form")]""", "designerTypeName", 0)]
    [DataRow("""[DesignerCategory("Form"), Designer($$"designerTypeName"$$, "designerBaseTypeName")]""", "designerTypeName", 0)]
    public void Attribute_ConstructorParameters(string attribute, string parameterName, int descriptorPosition)
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
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.AttributeArgument("DesignerAttribute", parameterName, descriptorPosition);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""[Obsolete($$"message")]""", "message", 0)]
    [DataRow("""[ObsoleteAttribute($$"message")]""", "message", 0)]
    [DataRow("""[ObsoleteAttribute($$"message", true)]""", "message", 0)]
    [DataRow("""[ObsoleteAttribute(error: true, message: $$"message")]""", "message", 0)]
    public void Attribute_ConstructorParameters_KnownType(string attribute, string parameterName, int parameterPosition)
    {
        var snippet = $$"""
            using System;

            {{attribute}}
            public class Test
            {
                public void M()
                {
                }
            }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.AttributeArgument(KnownType.System_ObsoleteAttribute, parameterName, parameterPosition);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Attribute_PropertySetter_WrongMethod()
    {
        var snippet = $$"""
            [Designer($$designerTypeName = "hey")]
            public class Test { }

            public class Designer : System.Attribute
            {
                public string designerTypeName { get; set; }
            }
            """;
        var context = ArgumentContextCS(snippet);

        // This should call AttributeProperty, not AttributeArgument.
        var descriptor = ArgumentDescriptor.AttributeArgument("Designer", "designerTypeName", 0);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Attribute_ConstructorParameters_WrongMethod()
    {
        var snippet = $$"""
            [Designer($$"value")]
            public class Test { }

            public class Designer(string designerTypeName) : System.Attribute { }
            """;
        var context = ArgumentContextCS(snippet);

        // This should call AttributeArgument, no AttributeProperty.
        var descriptor = ArgumentDescriptor.AttributeProperty("Designer", "designerTypeName");
        var result = MatchArgumentCS(context, descriptor);
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("""<Designer($$"designerTypeName")>""", "designerTypeName", 0)]
    [DataRow("""<DesignerAttribute($$"designerTypeName")>""", "designerTypeName", 0)]
    [DataRow("""<DesignerAttribute($$"designerTypeName", "designerBaseTypeName")>""", "designerTypeName", 0)]
    [DataRow("""<DesignerAttribute("designerTypeName", $$"designerBaseTypeName")>""", "designerBaseTypeName", 1)]
    public void Attribute_ConstructorParameters_VB(string attribute, string parameterName, int descriptorPosition)
    {
        var snippet = $$"""
            Imports System.ComponentModel

            {{attribute}}
            Public Class Test
            End Class
            """;
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.AttributeArgument("DesignerAttribute", parameterName, descriptorPosition);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true)]""", "AllowMultiple", true)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true, Inherited = true)]""", "AllowMultiple", true)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true, Inherited = true)]""", "Inherited", false)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  AllowMultiple = true, $$Inherited = true)]""", "Inherited", true)]
    public void Attribute_PropertySetter(string attribute, string propertyName, bool expected)
    {
        var snippet = $$"""
            using System;

            {{attribute}}
            public sealed class TestAttribute: Attribute
            {
            }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.AttributeProperty("AttributeUsage", propertyName);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
        if (result)
        {
            // The mapped parameter is the "value" parameter of the property set method.
            context.Parameter.Name.Should().Be("value");
            var method = context.Parameter.ContainingSymbol.Should().BeAssignableTo<IMethodSymbol>().Which;
            method.MethodKind.Should().Be(MethodKind.PropertySet);
            method.AssociatedSymbol.Should().BeAssignableTo<IPropertySymbol>().Which.Name.Should().Be(propertyName);
        }
    }

    [TestMethod]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true)]""", "AllowMultiple", true)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true, Inherited = true)]""", "AllowMultiple", true)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  $$AllowMultiple = true, Inherited = true)]""", "Inherited", false)]
    [DataRow("""[AttributeUsage(AttributeTargets.All,  AllowMultiple = true, $$Inherited = true)]""", "Inherited", true)]
    public void Attribute_PropertySetter_KnownType(string attribute, string propertyName, bool expected)
    {
        var snippet = $$"""
            using System;

            {{attribute}}
            public sealed class TestAttribute: Attribute
            {
            }
            """;
        var context = ArgumentContextCS(snippet);

        var descriptor = ArgumentDescriptor.AttributeProperty(KnownType.System_AttributeUsageAttribute, propertyName);
        var result = MatchArgumentCS(context, descriptor);
        result.Should().Be(expected);
        if (result)
        {
            // The mapped parameter is the "value" parameter of the property set method.
            context.Parameter.Name.Should().Be("value");
            var method = context.Parameter.ContainingSymbol.Should().BeAssignableTo<IMethodSymbol>().Which;
            method.MethodKind.Should().Be(MethodKind.PropertySet);
            method.AssociatedSymbol.Should().BeAssignableTo<IPropertySymbol>().Which.Name.Should().Be(propertyName);
        }
    }

    [TestMethod]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  $$AllowMultiple := true)>""", "AllowMultiple", true)]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  $$AllowMultiple := true, Inherited := true)>""", "AllowMultiple", true)]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  $$AllowMultiple := true, Inherited := true)>""", "Inherited", false)]
    [DataRow("""<AttributeUsage(AttributeTargets.All,  AllowMultiple := true, $$Inherited := true)>""", "Inherited", true)]
    public void Attribute_PropertySetter_VB(string attribute, string propertyName, bool expected)
    {
        var snippet = $$"""
            Imports System

            {{attribute}}
            Public NotInheritable Class TestAttribute
                Inherits Attribute
            End Class
            """;
        var context = ArgumentContextVB(snippet);

        var descriptor = ArgumentDescriptor.AttributeProperty("AttributeUsage", propertyName);
        var result = MatchArgumentVB(context, descriptor);
        result.Should().Be(expected);
        if (result)
        {
            // The mapped parameter is the "value" parameter of the property set method.
            context.Parameter.Name.Should().Be("value");
            var method = context.Parameter.ContainingSymbol.Should().BeAssignableTo<IMethodSymbol>().Which;
            method.MethodKind.Should().Be(MethodKind.PropertySet);
            method.AssociatedSymbol.Should().BeAssignableTo<IPropertySymbol>().Which.Name.Should().Be(propertyName);
        }
    }

    private static string WrapInMethodCS(string snippet) =>
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

    private static ArgumentContext ArgumentContextCS(string snippet) =>
        ArgumentContext(snippet, TestCompiler.CompileCS, typeof(CS.ArgumentSyntax), typeof(CS.AttributeArgumentSyntax));

    private static ArgumentContext ArgumentContextVB(string snippet) =>
        ArgumentContext(snippet, TestCompiler.CompileVB, typeof(VB.ArgumentSyntax));

    private static ArgumentContext ArgumentContext(string snippet,
        Func<string, MetadataReference[], (SyntaxTree Tree, SemanticModel Model)> compile, params Type[] descriptorNodeTypes)
    {
        var position = snippet.IndexOf("$$");
        if (position == -1)
        {
            throw new InvalidOperationException("The $$ maker was not found");
        }
        snippet = snippet.Replace("$$", string.Empty);
        var (tree, model) = compile(snippet, MetadataReferenceFacade.SystemCollections.Concat(MetadataReferenceFacade.SystemDiagnosticsProcess).ToArray());
        var node = tree.GetRoot()
            .DescendantNodesAndSelf(new TextSpan(position, 1)) // root.Find does not work with OmittedArgument
            .Reverse()
            .First(x => Array.Exists(descriptorNodeTypes, t => t.IsInstanceOfType(x)));
        return new(node, model);
    }

    private static bool MatchArgumentCS(ArgumentContext context, ArgumentDescriptor descriptor) =>
        MatchArgument<CSharpArgumentTracker, Microsoft.CodeAnalysis.CSharp.SyntaxKind>(context, descriptor);

    private static bool MatchArgumentVB(ArgumentContext context, ArgumentDescriptor descriptor) =>
        MatchArgument<VisualBasicArgumentTracker, Microsoft.CodeAnalysis.VisualBasic.SyntaxKind>(context, descriptor);

    private static bool MatchArgument<TTracker, TSyntaxKind>(ArgumentContext context, ArgumentDescriptor descriptor)
        where TTracker : ArgumentTracker<TSyntaxKind>, new()
        where TSyntaxKind : struct =>
        new TTracker().MatchArgument(descriptor)(context);
}
