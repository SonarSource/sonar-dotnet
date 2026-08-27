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

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.Core.Test.ShimLayer.Generated;

[TestClass]
public class ClassDeclarationSyntaxShimExtensionsTest
{
    [TestMethod]
    public void AddParameterListParameters_Method()
    {
        var newParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("ParameterAddedViaShimExtension")).WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
        var originalClass = SyntaxFactory.ClassDeclaration("Sample");

        var result = ClassDeclarationSyntaxShimExtensions.AddParameterListParameters(originalClass, [newParameter]);
        result.ParameterList.Parameters.Count.Should().Be(1);
        result.NormalizeWhitespace().ToString().Should().BeIgnoringLineEndings("""
            class Sample(int ParameterAddedViaShimExtension)
            {
            }
            """);
    }
}
