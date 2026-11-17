/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Test.TestFramework.Tests.MetadataReferences;

[TestClass]
public class CompilationExtensionsTest
{
    private const string Library = "public class Lib { }";

    [TestMethod]
    public void LanguageVersionString_Null() =>
        ((Func<string>)(() => CompilationExtensions.LanguageVersionString(null))).Should().Throw<NotSupportedException>().WithMessage("Not supported compilation type: ''");

    [TestMethod]
    public void ToPortableExecutableReference_ProvidesMetadataSymbols()
    {
        var reference = new SnippetCompiler(Library).Compilation.ToPortableExecutableReference();
        var lib = LibType(reference);

        reference.Should().BeAssignableTo<PortableExecutableReference>();
        lib.Locations.Should().ContainSingle().Which.IsInMetadata.Should().BeTrue();
        lib.DeclaringSyntaxReferences.Should().BeEmpty();
    }

    [TestMethod]
    public void ToMetadataReference_ProvidesSourceSymbols()
    {
#pragma warning disable T0047 // Deliberately asserts that CompilationReference exposes source symbols
        var lib = LibType(new SnippetCompiler(Library).Compilation.ToMetadataReference());
#pragma warning restore T0047

        lib.Locations.Should().ContainSingle().Which.IsInSource.Should().BeTrue();
        lib.DeclaringSyntaxReferences.Should().ContainSingle();
    }

    [TestMethod]
    public void EmitToImage_ReturnsLoadableAssembly() =>
        System.Reflection.Assembly.Load(new SnippetCompiler(Library).Compilation.EmitToImage()).GetType("Lib").Should().NotBeNull();

    [TestMethod]
    public void EmitToImage_CompilationErrors_Throws() =>
        ((Func<byte[]>)(() => new SnippetCompiler("public class Broken { UnknownType Field; }", ignoreErrors: true, AnalyzerLanguage.CSharp).Compilation.EmitToImage()))
            .Should()
            .Throw<Exception>();

    [TestMethod]
    public void CompileToReferenceCS_ProvidesMetadataSymbols() =>
        LibType(TestCompiler.CompileToReferenceCS(Library)).Locations.Should().ContainSingle().Which.IsInMetadata.Should().BeTrue();

    [TestMethod]
    public void CompileToReferenceVB_ProvidesMetadataSymbols()
    {
        var reference = TestCompiler.CompileToReferenceVB("Public Class Library\nEnd Class");
        var consumer = new SnippetCompiler("Public Class C\nEnd Class", ignoreErrors: false, AnalyzerLanguage.VisualBasic, [reference]).Compilation;

        ((IAssemblySymbol)consumer.GetAssemblyOrModuleSymbol(reference)).GlobalNamespace.GetTypeMembers("Library").Single().Locations.Should().ContainSingle().Which.IsInMetadata.Should().BeTrue();
    }

    private static INamedTypeSymbol LibType(MetadataReference reference)
    {
        var consumer = new SnippetCompiler("class C { }", reference).Compilation;
        return ((IAssemblySymbol)consumer.GetAssemblyOrModuleSymbol(reference)).GlobalNamespace.GetTypeMembers("Lib").Single();
    }
}
