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

using NSubstitute;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Core.Test.AnalysisContext;

[TestClass]
public class SonarCompilationStartAnalysisContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var compilation = TestHelper.CompileCS("// Nothing to see here").Model.Compilation;
        var options = AnalysisScaffolding.CreateOptions();
        var context = Substitute.For<CompilationStartAnalysisContext>(compilation, options, cancel);
        var sut = new SonarCompilationStartAnalysisContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.Compilation.Should().BeSameAs(compilation);
        sut.Options.Should().BeSameAs(options);
        sut.Cancel.Should().Be(cancel);
    }
}
