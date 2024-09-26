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

using SonarAnalyzer.Helpers;

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

    [DataTestMethod]
    [DataRow("NonExistingType", "NonExistingMember", false)]
    [DataRow("Sample", "NonExistingMember", false)]
    [DataRow("Sample", "_field", true)]
    [DataRow("Sample", "Property", true)]
    [DataRow("Sample", "MethodWithParameters", true)]
    public void IsMemberAvailable_WithoutTypeCheck(string typeName, string memberName, bool expectedResult)
    {
        var (_, semanticModel) = TestHelper.Compile(Snippet, false, AnalyzerLanguage.CSharp);
        semanticModel.Compilation.IsMemberAvailable<ISymbol>(new(typeName), memberName)
            .Should().Be(expectedResult);
    }

    [TestMethod]
    public void IsMemberAvailable_WithPredicate()
    {
        var (_, semanticModel) = TestHelper.Compile(Snippet, false, AnalyzerLanguage.CSharp);
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
        var (_, model) = TestHelper.Compile(string.Empty, false, AnalyzerLanguage.CSharp);

        ((Func<bool>)(() => model.Compilation.ReferencesAny()))
            .Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("Assemblies argument needs to be non-empty");
    }

    private static void ReferencesAny_ShouldBe(bool expected, IEnumerable<MetadataReference> compilationReferences, params KnownAssembly[] checkedAssemblies)
    {
        var (_, model) = TestHelper.Compile(string.Empty, false, AnalyzerLanguage.CSharp, compilationReferences.ToArray());
        model.Compilation.ReferencesAny(checkedAssemblies).Should().Be(expected);
    }
}
