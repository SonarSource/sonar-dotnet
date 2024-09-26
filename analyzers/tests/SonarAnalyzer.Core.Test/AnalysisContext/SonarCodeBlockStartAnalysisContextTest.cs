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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Core.Test.AnalysisContext;

[TestClass]
public class SonarCodeBlockStartAnalysisContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var codeBlock = SyntaxFactory.Block();
        var owningSymbol = Substitute.For<ISymbol>();
        var model = Substitute.For<SemanticModel>();
        var options = AnalysisScaffolding.CreateOptions();
        var context = Substitute.For<CodeBlockStartAnalysisContext<SyntaxKind>>(codeBlock, owningSymbol, model, options, cancel);
        var sut = new SonarCodeBlockStartAnalysisContext<SyntaxKind>(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.Compilation.Should().BeSameAs(model.Compilation);
        sut.Options.Should().BeSameAs(options);
        sut.Cancel.Should().Be(cancel);
        sut.CodeBlock.Should().BeSameAs(codeBlock);
        sut.OwningSymbol.Should().BeSameAs(owningSymbol);
        sut.SemanticModel.Should().BeSameAs(model);
    }
}
