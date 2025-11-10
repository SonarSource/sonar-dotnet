/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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

namespace SonarAnalyzer.Core.AnalysisContext.Test;

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
        sut.Model.Should().BeSameAs(model);
    }
}
