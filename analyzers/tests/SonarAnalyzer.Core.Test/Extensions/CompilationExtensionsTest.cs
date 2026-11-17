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

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class CompilationExtensionsTest
{
    private const string Snippet = """
        class Sample
        {
            int _field;
            string Property { get; }
            void MethodWithParameters(int arg1) { }
            void MethodWithParameters(int arg1, string arg2) { }
        }
        """;

    [TestMethod]
    public void GetTypeByMetadataName_NotDefined_ReturnsNull() =>
        Compile("class Consumer { }", "Consumer").GetTypeByMetadataName(new KnownType("Duplicate")).Should().BeNull();

    [TestMethod]
    public void GetTypeByMetadataName_DefinedOnce()
    {
        var compilation = Compile("class Consumer { }", "Consumer", ToReference("public class Duplicate { }", "Reference"));
        compilation.GetTypeByMetadataName(new KnownType("Duplicate")).ContainingAssembly.Name.Should().Be("Reference");
    }

    [TestMethod]
    public void GetTypeByMetadataName_DefinedInOwnAssemblyAndReference_PrefersOwnAssembly()
    {
        var compilation = Compile("public class Duplicate { }", "Consumer", ToReference("public class Duplicate { }", "Reference"));
        compilation.GetTypeByMetadataName(new KnownType("Duplicate")).ContainingAssembly.Name.Should().Be("Consumer");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void GetTypeByMetadataName_DefinedInSeveralReferences_ReturnsTheAccessibleOne(bool accessibleFirst)
    {
        var accessible = ToReference("public class Duplicate { }", "Accessible");
        var inaccessible = ToReference("internal class Duplicate { }", "Inaccessible");
        MetadataReference[] references = accessibleFirst ? [accessible, inaccessible] : [inaccessible, accessible];
        Compile("class Consumer { }", "Consumer", references).GetTypeByMetadataName(new KnownType("Duplicate")).ContainingAssembly.Name.Should().Be("Accessible");
    }

    [TestMethod]
    public void GetTypeByMetadataName_DefinedInSeveralReferences_InternalsVisibleTo_ReturnsTheAccessibleOne()
    {
        var visibleInternal = ToReference(
            """
            [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Consumer")]
            internal class Duplicate { }
            """,
            "VisibleInternal");
        var inaccessible = ToReference("internal class Duplicate { }", "Inaccessible");
        Compile("class Consumer { }", "Consumer", visibleInternal, inaccessible).GetTypeByMetadataName(new KnownType("Duplicate")).ContainingAssembly.Name.Should().Be("VisibleInternal");
    }

    [TestMethod]
    public void GetTypeByMetadataName_DefinedInSeveralReferences_Ambiguous_ReturnsNull()
    {
        var first = ToReference("public class Duplicate { }", "First");
        var second = ToReference("public class Duplicate { }", "Second");
        Compile("class Consumer { }", "Consumer", first, second).GetTypeByMetadataName(new KnownType("Duplicate")).Should().BeNull("both are accessible");
    }

    [TestMethod]
    public void GetTypeByMetadataName_DefinedInSeveralReferences_NoneAccessible_ReturnsNull()
    {
        var first = ToReference("internal class Duplicate { }", "First");
        var second = ToReference("internal class Duplicate { }", "Second");
        Compile("class Consumer { }", "Consumer", first, second).GetTypeByMetadataName(new KnownType("Duplicate")).Should().BeNull("none is accessible");
    }

    [TestMethod]
    [DataRow("NonExistingType", "NonExistingMember", false)]
    [DataRow("Sample", "NonExistingMember", false)]
    [DataRow("Sample", "_field", true)]
    [DataRow("Sample", "Property", true)]
    [DataRow("Sample", "MethodWithParameters", true)]
    public void IsMemberAvailable_WithoutTypeCheck(string typeName, string memberName, bool expectedResult)
    {
        var (_, semanticModel) = TestCompiler.Compile(Snippet, false, AnalyzerLanguage.CSharp);
        semanticModel.Compilation.IsMemberAvailable<ISymbol>(new(typeName), memberName)
            .Should().Be(expectedResult);
    }

    [TestMethod]
    public void IsMemberAvailable_WithPredicate()
    {
        var (_, semanticModel) = TestCompiler.Compile(Snippet, false, AnalyzerLanguage.CSharp);
        semanticModel.Compilation.IsMemberAvailable<IFieldSymbol>(new("Sample"), "_field", x => KnownType.System_Int32.Matches(x.Type))
            .Should().BeTrue();
        semanticModel.Compilation.IsMemberAvailable<IPropertySymbol>(new("Sample"), "Property", x => x is { IsReadOnly: true })
            .Should().BeTrue();
        semanticModel.Compilation.IsMemberAvailable<IMethodSymbol>(new("Sample"), "MethodWithParameters", x => x is { Parameters.Length: 2 })
            .Should().BeTrue();
    }

    [TestMethod]
    public void ReferencesAny_ShouldBeTrue()
    {
        Check(NuGetMetadataReference.NLog(), KnownAssembly.NLog);
        Check(NuGetMetadataReference.NLog().Concat(NuGetMetadataReference.NHibernate()), KnownAssembly.NLog);
        Check(NuGetMetadataReference.NLog(), KnownAssembly.NLog, KnownAssembly.NSubstitute);

        static void Check(IEnumerable<MetadataReference> compilationReferences, params KnownAssembly[] checkedAssemblies) =>
            ReferencesAny_ShouldBe(true, compilationReferences, checkedAssemblies);
    }

    [TestMethod]
    public void ReferencesAny_ShouldBeFalse()
    {
        Check(Enumerable.Empty<MetadataReference>(), KnownAssembly.NLog);
        Check(Enumerable.Empty<MetadataReference>(), KnownAssembly.NLog, KnownAssembly.NSubstitute);

        static void Check(IEnumerable<MetadataReference> compilationReferences, params KnownAssembly[] checkedAssemblies) =>
            ReferencesAny_ShouldBe(false, compilationReferences, checkedAssemblies);
    }

    [TestMethod]
    public void ReferencesAny_ShouldThrow()
    {
        var (_, model) = TestCompiler.Compile(string.Empty, false, AnalyzerLanguage.CSharp);

        ((Func<bool>)(() => model.Compilation.ReferencesAny()))
            .Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("Assemblies argument needs to be non-empty");
    }

    private static void ReferencesAny_ShouldBe(bool expected, IEnumerable<MetadataReference> compilationReferences, params KnownAssembly[] checkedAssemblies)
    {
        var (_, model) = TestCompiler.Compile(string.Empty, false, AnalyzerLanguage.CSharp, compilationReferences.ToArray());
        model.Compilation.ReferencesAny(checkedAssemblies).Should().Be(expected);
    }

    private static Compilation Compile(string code, string assemblyName, params MetadataReference[] references) =>
        new SnippetCompiler(code, references).Compilation.WithAssemblyName(assemblyName);

    private static PortableExecutableReference ToReference(string code, string assemblyName) =>
        Compile(code, assemblyName).ToPortableExecutableReference();
}
