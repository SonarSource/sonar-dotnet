/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Core.Test.AnalysisContext;

[TestClass]
public class SonarCodeBlockReportingContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var codeBlock = SyntaxFactory.Block();
        var owningSymbol = Substitute.For<ISymbol>();
        var model = Substitute.For<SemanticModel>();
        var options = AnalysisScaffolding.CreateOptions();
        var context = new CodeBlockAnalysisContext(codeBlock, owningSymbol, model, options, _ => { }, _ => true, cancel);
        var sut = new SonarCodeBlockReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.Tree.Should().BeSameAs(codeBlock.SyntaxTree);
        sut.Compilation.Should().BeSameAs(model.Compilation);
        sut.Options.Should().BeSameAs(options);
        sut.Cancel.Should().Be(cancel);
        sut.CodeBlock.Should().BeSameAs(codeBlock);
        sut.OwningSymbol.Should().BeSameAs(owningSymbol);
        sut.SemanticModel.Should().BeSameAs(model);
    }
}
