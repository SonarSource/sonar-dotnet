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
    [DataTestMethod]
    [DataRow("NonExistingType", "NonExistingMember", null, false)]
    [DataRow("Sample", "NonExistingMember", null, false)]
    [DataRow("Sample", "_field", null, true)]
    [DataRow("Sample", "_field", "System.Int32", true)]
    [DataRow("Sample", "_field", "System.String", false)]
    [DataRow("Sample", "Property", null, true)]
    [DataRow("Sample", "Property", "System.String", true)]
    [DataRow("Sample", "Property", "System.Int32", false)]
    [DataRow("Sample", "Method", null, true)]
    [DataRow("Sample", "Method", "System.Void", true)]
    [DataRow("Sample", "Method", "System.Int32", false)]
    public void IsMemberAvailable_NoParameters(string containingType, string memberName, string memberType, bool expectedResult)
    {
        var snippet = """
            class Sample
            {
                int _field;
                string Property { get; set; }
                void Method() { }
            }
            """;
        IsMemberAvailable(snippet, AnalyzerLanguage.CSharp, containingType, memberName, memberType)
            .Should().Be(expectedResult);
    }

    [DataTestMethod]
    [DataRow(true, "System.Int32")]
    [DataRow(true, "System.Int32", "System.String")]
    [DataRow(false, "System.String", "System.Int32")]
    [DataRow(false, "System.Int32", "System.String", "System.Int32")]
    [DataRow(false)]
    public void IsMemberAvailable_MethodParameters(bool expectedResult, params string[] parameterTypes)
    {
        var snippet = """
            class Sample
            {
                void MethodWithParameters(int arg1) { }
                void MethodWithParameters(int arg1, string arg2) { }
            }
            """;
        IsMemberAvailable(snippet, AnalyzerLanguage.CSharp, "Sample", "MethodWithParameters", null, parameterTypes)
            .Should().Be(expectedResult);
    }

    private static bool IsMemberAvailable(
        string snippet,
        AnalyzerLanguage language,
        string containingType,
        string memberName,
        string memberType,
        params string[] parameterTypes)
    {
        var (_, semanticModel) = TestHelper.Compile(snippet, false, language);
        var arguments = parameterTypes.Select(x => new KnownType(x)).ToArray();
        return semanticModel.Compilation.IsMemberAvailable(new(containingType), memberName, memberType != null ? new(memberType) : null, arguments);
    }
}
