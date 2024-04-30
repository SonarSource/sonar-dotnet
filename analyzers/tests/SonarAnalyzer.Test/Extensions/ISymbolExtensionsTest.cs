/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

extern alias common;
extern alias csharp;
extern alias vbnet;

using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using CodeAnalysisCS = Microsoft.CodeAnalysis.CSharp;
using CodeAnalysisVB = Microsoft.CodeAnalysis.VisualBasic;
using ISymbolExtensionsCommon = common::SonarAnalyzer.Extensions.ISymbolExtensions;
using ISymbolExtensionsCS = csharp::SonarAnalyzer.Extensions.ISymbolExtensions;
using ISymbolExtensionsVB = vbnet::SonarAnalyzer.Extensions.ISymbolExtensions;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class ISymbolExtensionsTest
{
    [TestMethod]
    public void GetDescendantNodes_ForNullSourceTree_ReturnsEmpty_VB() =>
        ISymbolExtensionsVB.GetDescendantNodes(Location.None, CodeAnalysisVB.SyntaxFactory.ModifiedIdentifier("a")).Should().BeEmpty();

    [TestMethod]
    public void GetDescendantNodes_ForDifferentSyntaxTrees_ReturnsEmpty_VB()
    {
        var first = CodeAnalysisVB.SyntaxFactory.ParseSyntaxTree("Dim a As String");
        var identifier = first.Single<ModifiedIdentifierSyntax>();

        var second = CodeAnalysisVB.SyntaxFactory.ParseSyntaxTree("Dim a As String");
        ISymbolExtensionsVB.GetDescendantNodes(identifier.GetLocation(), second.GetRoot()).Should().BeEmpty();
    }

    [TestMethod]
    public void GetDescendantNodes_ForMissingVariableDeclarator_ReturnsEmpty_VB()
    {
        var tree = CodeAnalysisVB.SyntaxFactory.ParseSyntaxTree(@"new FileSystemAccessRule(""User"", FileSystemRights.ListDirectory, AccessControlType.Allow)");
        ISymbolExtensionsVB.GetDescendantNodes(tree.GetRoot().GetLocation(), tree.GetRoot()).Should().BeEmpty();
    }

    [TestMethod]
    public void IsInType_Null_KnownType() =>
        ISymbolExtensionsCommon.IsInType(null, KnownType.System_Boolean).Should().BeFalse();

    [TestMethod]
    public void IsInType_Null_TypeSymbol() =>
        ISymbolExtensionsCommon.IsInType(null, (ITypeSymbol)null).Should().BeFalse();

    [TestMethod]
    public void IsInType_Null_ArrayOfTypeSymbols() =>
        ISymbolExtensionsCommon.IsInType(null, []).Should().BeFalse();

    [DataTestMethod]
    [DataRow("{ get; set; }")]
    [DataRow("{ get; }")]
    [DataRow("{ get; } = string.Empty;")]
    [DataRow("{ get; set; } = string.Empty;")]
#if NET
    [DataRow("{ get; init; }")]
#endif
    public void IsAutoProperty_AutoProperty_CS(string getterSetter)
    {
        var code = $$"""
            public class Sample
            {
                public string SymbolMember {{getterSetter}}
            }
            """;
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.CSharp)).Should().BeTrue();
    }

    [TestMethod]
    public void IsAutoProperty_AutoProperty_VB()
    {
        const string code = """
            Public Class Sample

                Public Property SymbolMember As String

            End Class
            """;
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.VisualBasic)).Should().BeTrue();
    }

    [TestMethod]
    public void IsAutoProperty_ExplicitProperty_CS()
    {
        const string code = """
            public class Sample
            {
                private string _SymbolMember; // Try to confuse the method with auto-like implementation

                public string SymbolMember
                {
                    get => _SymbolMember;
                    set { _SymbolMember = value; }
                }
            }
            """;
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.CSharp)).Should().BeFalse();
    }

    [TestMethod]
    public void IsAutoProperty_ExplicitProperty_VB()
    {
        const string code = """
            Public Class Sample

                Private _SymbolMember As String ' Try to confuse the method with auto-like implementation

                Public Property SymbolMember As String
                    Get
                        Return _SymbolMember
                    End Get
                    Set(value As String)
                        _SymbolMember = value
                    End Set
                End Property

            End Class
            """;
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.VisualBasic)).Should().BeFalse();
    }

    [TestMethod]
    public void IsAutoProperty_NonpropertySymbol_CS()
    {
        const string code = """
            public class Sample
            {
                public void SymbolMember() { }
            }
            """;
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.CSharp)).Should().BeFalse();
    }

    [TestMethod]
    public void IsAutoProperty_NonpropertySymbol_VB()
    {
        const string code = """
            Public Class Sample

                Public Sub SymbolMember()
                End Sub

            End Class
            """;
        ISymbolExtensionsCommon.IsAutoProperty(CreateSymbol(code, AnalyzerLanguage.VisualBasic)).Should().BeFalse();
    }

#if NET

    [DataTestMethod]
    [DataRow("class SymbolMember();", true)]
    [DataRow("class SymbolMember() { }", true)]
    [DataRow("class SymbolMember(int a) { }", true)]
    [DataRow("class SymbolMember { }", false)]
    [DataRow("class SymbolMember(int a) { @SymbolMember() : this(1) { } };", true)]
    [DataRow("class SymbolMember { SymbolMember() { } };", false)]
    [DataRow("class Base(int i); class SymbolMember() : Base(1);", true)]
    [DataRow("""
        class Base(int i);
        class SymbolMember : Base
        {
            @SymbolMember() : base(1) { }
        }
        """, false)]
    [DataRow("struct SymbolMember();", true)]
    [DataRow("struct SymbolMember() { }", true)]
    [DataRow("struct SymbolMember(int a) { }", true)]
    [DataRow("struct SymbolMember { }", false)]
    [DataRow("struct SymbolMember(int a) { public @SymbolMember() : this(1) { } };", true)]
    [DataRow("struct SymbolMember { public @SymbolMember() { } };", false)]
    [DataRow("record SymbolMember();", true)]
    [DataRow("record SymbolMember() { }", true)]
    [DataRow("record SymbolMember(int a) { }", true)]
    [DataRow("record SymbolMember { }", false)]
    [DataRow("record SymbolMember(int a) { @SymbolMember() : this(1) { } };", true)]
    [DataRow("record SymbolMember { SymbolMember() { } };", false)]
    [DataRow("record struct SymbolMember();", true)]
    [DataRow("record struct SymbolMember() { }", true)]
    [DataRow("record struct SymbolMember(int a) { }", true)]
    [DataRow("record struct SymbolMember { }", false)]
    [DataRow("record struct SymbolMember(int a) { public @SymbolMember() : this(1) { } };", true)]
    [DataRow("record struct SymbolMember { public @SymbolMember() { } };", false)]
    [DataRow("record class SymbolMember();", true)]
    [DataRow("record class SymbolMember() { }", true)]
    [DataRow("record class SymbolMember(int a) { }", true)]
    [DataRow("record class SymbolMember { }", false)]
    [DataRow("record class SymbolMember(int a) { @SymbolMember() : this(1) { } };", true)]
    [DataRow("record class SymbolMember { @SymbolMember() { } };", false)]
    [DataRow("readonly struct SymbolMember();", true)]
    [DataRow("readonly struct SymbolMember() { }", true)]
    [DataRow("readonly struct SymbolMember(int a) { }", true)]
    [DataRow("readonly struct SymbolMember { }", false)]
    [DataRow("readonly struct SymbolMember(int a) { public @SymbolMember() : this(1) { } };", true)]
    [DataRow("readonly struct SymbolMember { public @SymbolMember() { } };", false)]
    public void IsPrimaryConstructor_CS(string code, bool hasPrimaryConstructor)
    {
        var typeSymbol = (INamedTypeSymbol)CreateSymbol(code, AnalyzerLanguage.CSharp, new CodeAnalysisCS.CSharpParseOptions(CodeAnalysisCS.LanguageVersion.Latest));
        var methodSymbols = typeSymbol.GetMembers().OfType<IMethodSymbol>();

        methodSymbols.Count(ISymbolExtensionsCS.IsPrimaryConstructor).Should().Be(hasPrimaryConstructor ? 1 : 0);
    }

#endif

    private static ISymbol CreateSymbol(string snippet, AnalyzerLanguage language, ParseOptions parseOptions = null)
    {
        var (tree, semanticModel) = TestHelper.Compile(snippet, false, language, parseOptions: parseOptions);
        var node = tree.GetRoot().DescendantNodes().Last(x => x.ToString().Contains(" SymbolMember"));
        return semanticModel.GetDeclaredSymbol(node);
    }
}
