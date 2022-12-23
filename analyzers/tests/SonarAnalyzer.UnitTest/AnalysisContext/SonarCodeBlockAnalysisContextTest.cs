/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using Moq;
using SonarAnalyzer.AnalysisContext;
using RoslynAnalysisContext = Microsoft.CodeAnalysis.Diagnostics.AnalysisContext;

namespace SonarAnalyzer.UnitTest.AnalysisContext;

[TestClass]
public class SonarCodeBlockAnalysisContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var analysisContext = new SonarAnalysisContext(Mock.Of<RoslynAnalysisContext>(), Enumerable.Empty<DiagnosticDescriptor>());
        var cancel = new CancellationToken(true);
        var codeBlock = SyntaxFactory.Block();
        var owningSymbol = Mock.Of<ISymbol>();
        var model = Mock.Of<SemanticModel>();
        var context = new CodeBlockAnalysisContext(codeBlock, owningSymbol, model, null, _ => { }, _ => true, cancel);
        var sut = new SonarCodeBlockAnalysisContext(analysisContext, context);

        sut.Cancel.Should().Be(cancel);
        sut.CodeBlock.Should().Be(codeBlock);
        sut.OwningSymbol.Should().Be(owningSymbol);
        sut.SemanticModel.Should().Be(model);
    }
}
