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

namespace SonarAnalyzer.Core.Test.Semantics.Extensions;

[TestClass]
public class INamedTypeSymbolExtensionsTest
{
    // One real assembly for every entry of DependencyInjectionManagedTypes and DependencyInjectionTypeAttributes.
    private static readonly MetadataReference[] DependencyInjectionFrameworkReferences =
        [
            ..NuGetMetadataReference.Package("MassTransit.Abstractions", "8.3.7"),
            ..NuGetMetadataReference.Package("MediatR", "12.4.1"),
            ..NuGetMetadataReference.Package("MediatR.Contracts", "2.0.1"),
            ..NuGetMetadataReference.MicrosoftAspNetSignalRCore(),
            ..NuGetMetadataReference.Package("Microsoft.AspNetCore.Authorization", "2.2.0"),
            ..NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(),
            ..NuGetMetadataReference.MicrosoftAspNetCoreMvcAbstractions("2.2.0"),
            ..NuGetMetadataReference.MicrosoftAspNetCoreMvcCore("2.2.0"),
            ..NuGetMetadataReference.Package("Microsoft.AspNetCore.Mvc.RazorPages", "2.2.0"),
            ..NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures("2.2.0"),
            ..NuGetMetadataReference.Package("Microsoft.AspNetCore.Razor", "2.2.0"),
            ..NuGetMetadataReference.MicrosoftAspNetCoreMvcRazorRuntime(),
            ..NuGetMetadataReference.Package("Microsoft.AspNetCore.SignalR.Core", "1.1.0"),
            ..NuGetMetadataReference.Package("Microsoft.Extensions.Hosting.Abstractions", "8.0.1"),
            ..NuGetMetadataReference.MicrosoftAspNetMvc("5.2.9"),
            ..NuGetMetadataReference.MicrosoftNetWebApiCore("5.2.9")
        ];

    [TestMethod]
    public void GetAllNamedTypesForNamedType_WhenSymbolIsNull_ReturnsEmpty() =>
        ((INamedTypeSymbol)null).AllNamedTypes.Should().BeEmpty();

    // Guards against a KnownType that no framework declares: a wrong namespace, arity, or generic parameter name silently stops matching,
    // and a test declaring the framework type itself cannot detect it, because such a declaration is written from the KnownType.
    [TestMethod]
    public void DependencyInjectionTypes_MatchTheRealFrameworkDeclarations()
    {
        var compilation = TestCompiler.CompileCS("// Empty file", DependencyInjectionFrameworkReferences).Model.Compilation;

        foreach (var knownType in INamedTypeSymbolExtensions.DependencyInjectionManagedTypes.Concat(INamedTypeSymbolExtensions.DependencyInjectionTypeAttributes))
        {
            var symbol = compilation.GetTypeByMetadataName(knownType.MetadataName);
            symbol.Should().NotBeNull("{0} should be declared by one of the referenced framework assemblies", knownType.MetadataName);
            knownType.Matches(symbol).Should().BeTrue("{0} should match the real declaration, including its generic parameter names", knownType.MetadataName);
        }
    }

    [TestMethod]
    [DataRow(TypeKind.Array, "array")]
    [DataRow(TypeKind.Class, "class")]
    [DataRow(TypeKind.Delegate, "delegate")]
    [DataRow(TypeKind.Dynamic, "dynamic")]
    [DataRow(TypeKind.Enum, "enum")]
    [DataRow(TypeKind.Error, "error")]
    [DataRow(TypeKind.FunctionPointer, "function pointer")]
    [DataRow(TypeKind.Interface, "interface")]
    [DataRow(TypeKind.Module, "module")]
    [DataRow(TypeKind.Pointer, "pointer")]
    [DataRow(TypeKind.Struct, "struct")]
    [DataRow(TypeKind.Submission, "submission")]
    [DataRow(TypeKind.TypeParameter, "type parameter")]
    [DataRow(TypeKind.Unknown, "unknown")]
    public void GetClassification_NamedTypes(TypeKind typeKind, string expected)
    {
        var symbol = Substitute.For<INamedTypeSymbol>();
        symbol.Kind.Returns(SymbolKind.NamedType);
        symbol.TypeKind.Returns(typeKind);
        symbol.IsRecord.Returns(false);

        symbol.Classification.Should().Be(expected);
    }

    [TestMethod]
    public void GetClassification_NamedType_Unknown()
    {
        var symbol = Substitute.For<INamedTypeSymbol>();
        symbol.Kind.Returns(SymbolKind.NamedType);
        symbol.TypeKind.Returns((TypeKind)255);
#if DEBUG
        new Action(() => _ = symbol.Classification).Should().Throw<NotSupportedException>();
#else
        symbol.Classification.Should().Be("type");
#endif
    }

    [TestMethod]
    [DataRow(TypeKind.Class, "record")]
    [DataRow(TypeKind.Struct, "record struct")]
    public void GetClassification_Record(TypeKind typeKind, string expected)
    {
        var symbol = Substitute.For<INamedTypeSymbol>();
        symbol.Kind.Returns(SymbolKind.NamedType);
        symbol.TypeKind.Returns(typeKind);
        symbol.IsRecord.Returns(true);

        symbol.Classification.Should().Be(expected);
    }
}
