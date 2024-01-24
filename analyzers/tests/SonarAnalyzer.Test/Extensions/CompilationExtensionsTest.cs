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

using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.Test.Extensions;

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
    public void IsMemberAvailable_WithoutPredicate(string typeName, string memberName, bool expectedResult)
    {
        var (_, semanticModel) = TestHelper.Compile(Snippet, false, AnalyzerLanguage.CSharp);
        semanticModel.Compilation.IsMemberAvailable(new(typeName), memberName)
            .Should().Be(expectedResult);
    }

    [TestMethod]
    public void IsMemberAvailable_WithPredicate()
    {
        var (_, semanticModel) = TestHelper.Compile(Snippet, false, AnalyzerLanguage.CSharp);
        semanticModel.Compilation.IsMemberAvailable(new("Sample"), "Property", x => x is IPropertySymbol { IsReadOnly: true })
            .Should().BeTrue();
        semanticModel.Compilation.IsMemberAvailable(new("Sample"), "MethodWithParameters", x => x is IMethodSymbol { Parameters.Length: 2 })
            .Should().BeTrue();
    }
}
