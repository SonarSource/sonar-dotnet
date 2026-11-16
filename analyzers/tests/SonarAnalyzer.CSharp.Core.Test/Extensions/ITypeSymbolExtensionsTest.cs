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

using System.IO;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class ITypeSymbolExtensionsTest
{
    private static readonly MetadataReference[] RuntimeReferences = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))
        .Split(Path.PathSeparator)
        .Where(x => Path.GetFileName(x).StartsWith("System.") || Path.GetFileName(x) == "netstandard.dll")
        .Select(x => (MetadataReference)MetadataReference.CreateFromFile(x))
        .ToArray();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void CaseOfUnions_SourceCaseType()
    {
        var compilation = Compile("""
            public union Result(Success, Failure);
            public class Success { }
            public class Failure { }
            public class NotACase { }
            """);
        UnionNames(compilation, "Success").Should().BeEquivalentTo("Result");
        UnionNames(compilation, "Failure").Should().BeEquivalentTo("Result");
        UnionNames(compilation, "NotACase").Should().BeEmpty();
        UnionNames(compilation, "Result").Should().BeEmpty();
        Type(compilation, "Success").IsUnionCaseType(compilation).Should().BeTrue();
        Type(compilation, "NotACase").IsUnionCaseType(compilation).Should().BeFalse();
    }

    [TestMethod]
    public void CaseOfUnions_CaseOfTwoUnions()
    {
        var compilation = Compile("""
            public union First(Shared, Other);
            public union Second(Shared, int);
            public class Shared { }
            public class Other { }
            """);
        UnionNames(compilation, "Shared").Should().BeEquivalentTo("First", "Second");
        UnionNames(compilation, "Other").Should().BeEquivalentTo("First");
    }

    [TestMethod]
    public void CaseOfUnions_HandWrittenUnionStruct()
    {
        var compilation = Compile("""
            using System.Runtime.CompilerServices;

            [Union]
            public struct Manual : IUnion
            {
                public Manual(Case1 value) { Value = value; }
                public Manual(Case2 value) { Value = value; }
                public object Value { get; }
            }
            public class Case1 { }
            public class Case2 { }
            public class NotACase { }
            """);
        UnionNames(compilation, "Case1").Should().BeEquivalentTo("Manual");
        UnionNames(compilation, "Case2").Should().BeEquivalentTo("Manual");
        UnionNames(compilation, "NotACase").Should().BeEmpty();
    }

    [TestMethod]
    public void CaseOfUnions_MetadataCaseType()
    {
        var compilation = Compile("public union R(int, bool);");
        compilation.GetSpecialType(SpecialType.System_Int32).CaseOfUnions(compilation).Select(x => x.Name).Should().BeEquivalentTo("R");
        compilation.GetSpecialType(SpecialType.System_String).IsUnionCaseType(compilation).Should().BeFalse();
    }

    [TestMethod]
    public void CaseOfUnions_NoUnions()
    {
        var compilation = Compile("public class A { } public class B { }");
        UnionNames(compilation, "A").Should().BeEmpty();
        UnionNames(compilation, "B").Should().BeEmpty();
        compilation.GetSpecialType(SpecialType.System_Int32).IsUnionCaseType(compilation).Should().BeFalse();
    }

    [TestMethod]
    public void CaseOfUnions_UnionInReferencedAssembly_PEReference_IsNotFound()
    {
        var library = Compile("public union Result(int, bool);", "Library");
        using var stream = new MemoryStream();
        library.Emit(stream, cancellationToken: TestContext.CancellationToken).Success.Should().BeTrue();
        var compilation = Compile("public class Consumer { }", "Consumer", MetadataReference.CreateFromImage(stream.ToArray()));
        compilation.GetTypeByMetadataName("Result").Should().NotBeNull("the union is visible through the reference");
        compilation.GetSpecialType(SpecialType.System_Int32).CaseOfUnions(compilation).Should().BeEmpty();
    }

    [TestMethod]
    public void CaseOfUnions_UnionInReferencedAssembly_SourceReference_IsNotFound()
    {
        var library = Compile("public union Result(int, bool);", "Library");
        var compilation = Compile("public class Consumer { }", "Consumer", library.ToMetadataReference());
        compilation.GetTypeByMetadataName("Result").Should().NotBeNull("the union is visible through the reference");
        compilation.GetSpecialType(SpecialType.System_Int32).CaseOfUnions(compilation).Should().BeEmpty();
    }

    [TestMethod]
    public void CaseOfUnions_Generics()
    {
        var compilation = Compile("""
            public union GenericResult<T>(Ok<T>, Error);
            public union Closed(Ok<int>, Error);
            public class Ok<T> { }
            public class Error { }
            """);
        UnionNames(compilation, "Ok`1").Should().BeEquivalentTo("GenericResult", "Closed");
        UnionNames(compilation, "Error").Should().BeEquivalentTo("GenericResult", "Closed");
        // Over-approximation: Ok<string> matches through OriginalDefinition, so it also reports the unions that list Ok<int>
        Type(compilation, "Ok`1")
            .Construct(compilation.GetSpecialType(SpecialType.System_String))
            .CaseOfUnions(compilation)
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo("GenericResult", "Closed");
    }

    [TestMethod]
    public void CaseOfUnions_TypeParameterCase()
    {
        var compilation = Compile("public union U<T>(T, string);");
        Type(compilation, "U`1").TypeParameters.Single().CaseOfUnions(compilation).Should().ContainSingle().Which.Name.Should().Be("U");
        compilation.GetSpecialType(SpecialType.System_String).CaseOfUnions(compilation).Should().ContainSingle().Which.Name.Should().Be("U");
    }

    [TestMethod]
    public void CaseOfUnions_CaseTypeNestedInGenericClass()
    {
        var compilation = Compile("""
            public union R(Outer<int>.Inner, string);
            public class Outer<T> { public class Inner { } }
            """);
        Type(compilation, "Outer`1").GetTypeMembers("Inner").Single().CaseOfUnions(compilation).Should().ContainSingle().Which.Name.Should().Be("R");
    }

    [TestMethod]
    public void CaseOfUnions_UnionNestedInType()
    {
        var compilation = Compile("""
            public class Outer
            {
                public union R(A, B);
                public class Inner { public union S(A, int); }
            }
            public class A { }
            public class B { }
            """);
        UnionNames(compilation, "A").Should().BeEquivalentTo("R", "S");
        UnionNames(compilation, "B").Should().BeEquivalentTo("R");
    }

    private static CSharpCompilation Compile(string code, string assemblyName = "Test", params MetadataReference[] references) =>
        CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersionEx.Preview))],
            RuntimeReferences.Concat(references),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static INamedTypeSymbol Type(Compilation compilation, string metadataName) =>
        compilation.GetTypeByMetadataName(metadataName);

    private static string[] UnionNames(Compilation compilation, string metadataName) =>
        Type(compilation, metadataName).CaseOfUnions(compilation).Select(x => x.Name).ToArray();
}
