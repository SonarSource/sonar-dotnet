/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Core.Test.Semantics;

[TestClass]
public class KnownTypeTest
{
    [TestMethod]
    public void Matches_TypeSymbolIsNull_ThrowsArgumentNullException() =>
        new KnownType("typeName").Invoking(x => x.Matches(null)).Should().Throw<ArgumentNullException>();

    [TestMethod]
    public void Constructor_InvalidName_Throws() =>
        ((Func<KnownType>)(() => new KnownType("Invalid.Order+Of.Separators"))).Should().Throw<ArgumentException>();

    [DataTestMethod]
    [DataRow("System.Action", true, "System.Action", false)]
    [DataRow("System.DateTime", false, "System.Action", false)]
    [DataRow("System.Collections.ArrayList", true, "System.Collections.ArrayList", false)]
    [DataRow("System.Threading.Timer", false, "System.Timers.Timer", false)]
    [DataRow("string", true, "System.String", false)]
    [DataRow("byte", true, "System.Byte", false)]
    [DataRow("string[]", true, "System.String", true)]
    [DataRow("System.String[]", true, "System.String", true)]
    [DataRow("byte[]", true, "System.Byte", true)]
    [DataRow("System.Byte[]", true, "System.Byte", true)]
    [DataRow("System.Exception[]", false, "Exceptions.Exception", true)]
    [DataRow("System.Action[]", false, "System.Action", false)]
    [DataRow("System.Action<T>", false, "System.Action", false)]
    [DataRow("System.Action<T>", true, "System.Action", false, "T")]
    [DataRow("System.Action<T1>", false, "System.Action", true, "T2")]
    [DataRow("System.Action<T1, T2>", true, "System.Action", false, "T1", "T2")]
    [DataRow("System.Action<T1, T2>", false, "System.Action", true, "T1", "T2")]
    [DataRow("System.Action<T1, T2>", false, "System.Action", false, "T2", "T1")]
    [DataRow("System.Action<T1, T2>", false, "System.Action", true, "T1")]
    [DataRow("System.Action", false, "System.Action", false, "T")]
    [DataRow("System.Action<T>", false, "System.Action", true, "T")]
    [DataRow("OuterNamespace.InnerNamespace.TheType", true, "OuterNamespace.InnerNamespace.TheType", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce", false, "OuterNamespace.InnerNamespace.TheType", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce", true, "OuterNamespace.InnerNamespace.TheType+NestedOnce", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce.NestedTwice", false, "OuterNamespace.InnerNamespace.TheType+NestedOnce", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce.NestedTwice", true, "OuterNamespace.InnerNamespace.TheType+NestedOnce+NestedTwice", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce", false, "NestedOnce", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType", false, "OuterNamespace.InnerNamespace.TheType+NestedOnce", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedGeneric<T1>", true, "OuterNamespace.InnerNamespace.TheType+NestedGeneric", false, "T")]
    [DataRow("OuterNamespace.InnerNamespace.GenericType<T1>.Nested", true, "OuterNamespace.InnerNamespace.GenericType+Nested", false)]
    [DataRow("OuterNamespace.InnerNamespace.GenericType<T1>", true, "OuterNamespace.InnerNamespace.GenericType", false, "TOuter")]
    [DataRow("OuterNamespace.InnerNamespace.GenericType<T1>.NestedGeneric<T2>", true, "OuterNamespace.InnerNamespace.GenericType+NestedGeneric", false, "TInner")]  // Only the most-inner Generic type is relevant
    [DataRow("OuterNamespace.InnerNamespace.GenericType<T1>.NestedGeneric<T2>.NestedTwice", true, "OuterNamespace.InnerNamespace.GenericType+NestedGeneric+NestedTwice", false)]
    public void Matches_TypeSymbol_CS(string symbolName, bool expectedMatch, string fullTypeName, bool isArray, params string[] genericParameters) =>
        new KnownType(fullTypeName, genericParameters) { IsArray = isArray }
            .Matches(GetSymbol_CS(symbolName))
            .Should().Be(expectedMatch);

    [DataTestMethod]
    [DataRow("System.Collections.Generic.IDictionary(Of TKey, TValue)", false, "System.Collections.Generic.IDictionary", false)]
    [DataRow("System.Collections.Generic.IDictionary(Of TKey, TValue)", false, "System.Collections.Generic.IDictionary", false, "TKey")]
    [DataRow("System.Collections.Generic.IDictionary(Of TKey, TValue)", true, "System.Collections.Generic.IDictionary", false, "TKey", "TValue")]
    [DataRow("System.Collections.Generic.IDictionary(Of TKey, TValue)", false, "System.Collections.Generic.IDictionary", false, "TValue", "TKey")]
    [DataRow("String()", true, "System.String", true)]
    [DataRow("String()", false, "System.Byte", true)]
    [DataRow("OuterNamespace.InnerNamespace.TheType", true, "OuterNamespace.InnerNamespace.TheType", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce", false, "OuterNamespace.InnerNamespace.TheType", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce", true, "OuterNamespace.InnerNamespace.TheType+NestedOnce", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce.NestedTwice", false, "OuterNamespace.InnerNamespace.TheType+NestedOnce", false)]
    [DataRow("OuterNamespace.InnerNamespace.TheType.NestedOnce.NestedTwice", true, "OuterNamespace.InnerNamespace.TheType+NestedOnce+NestedTwice", false)]
    public void Matches_TypeSymbol_VB(string symbolName, bool expectedMatch, string fullTypeName, bool isArray, params string[] genericParameters) =>
        new KnownType(fullTypeName, genericParameters) { IsArray = isArray }
            .Matches(GetSymbol_VB(symbolName))
            .Should().Be(expectedMatch);

    [DataTestMethod]
    [DataRow("System.String", "System.String", false)]
    [DataRow("System.String[]", "System.String", true)]
    [DataRow("System.Action<T>", "System.Action", false, "T")]
    [DataRow("System.Action<T1, T2>", "System.Action", false, "T1", "T2")]
    [DataRow("System.Action<T1, T2>[]", "System.Action", true, "T1", "T2")]
    public void DebuggerDisplay(string expectedResult, string fullTypeName, bool isArray, params string[] genericParameters) =>
        new KnownType(fullTypeName, genericParameters) { IsArray = isArray }.DebuggerDisplay.Should().Be(expectedResult);

    private static ITypeSymbol GetSymbol_CS(string type)
    {
        var (tree, model) = TestCompiler.CompileCS($$"""
            namespace Exceptions { public class Exception { } }
            public class Test<T, T1, T2, T3> { public {{type}} Value; }
            namespace OuterNamespace
            {
                namespace InnerNamespace
                {
                    public class TheType
                    {
                        public class NestedOnce
                        {
                            public class NestedTwice { }
                        }
                        public class NestedGeneric<T> { }
                    }
                    public class GenericType<TOuter>
                    {
                        public class Nested { }
                        public class NestedGeneric<TInner>
                        {
                            public class NestedTwice { }
                        }
                    }
                }
            }
            """);
        var expression = tree.Single<CS.VariableDeclaratorSyntax>();
        return model.GetDeclaredSymbol(expression).GetSymbolType();
    }

    private static ITypeSymbol GetSymbol_VB(string type)
    {
        var (tree, model) = TestCompiler.CompileVB($"""
            Public Class Test(Of TKey, TValue) : Public Value As {type} : End Class
            Namespace OuterNamespace
                Namespace InnerNamespace
                    Public Class TheType
                        Public Class NestedOnce
                            Public Class NestedTwice
                            End Class
                        End Class
                    End Class
                End Namespace
            End Namespace
            """);
        var expression = tree.Single<VB.ModifiedIdentifierSyntax>();
        return model.GetDeclaredSymbol(expression).GetSymbolType();
    }
}
