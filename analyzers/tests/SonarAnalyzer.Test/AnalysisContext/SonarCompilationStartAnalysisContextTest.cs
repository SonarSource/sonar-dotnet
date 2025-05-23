﻿/*
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

using NSubstitute;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.Test.AnalysisContext;

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
