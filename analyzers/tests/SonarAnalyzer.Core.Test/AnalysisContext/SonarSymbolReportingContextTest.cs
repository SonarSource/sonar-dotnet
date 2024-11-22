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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Core.Test.AnalysisContext;

[TestClass]
public class SonarSymbolReportingContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var (tree, model) = TestHelper.CompileCS("public class Sample { }");
        var options = AnalysisScaffolding.CreateOptions();
        var symbol = model.GetDeclaredSymbol(tree.Single<ClassDeclarationSyntax>());
        var context = new SymbolAnalysisContext(symbol, model.Compilation, options, _ => { }, _ => true, cancel);
        var sut = new SonarSymbolReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.Compilation.Should().BeSameAs(model.Compilation);
        sut.Options.Should().BeSameAs(options);
        sut.Cancel.Should().Be(cancel);
        sut.Symbol.Should().BeSameAs(symbol);
    }
}
