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

using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Analyzers;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class TypeExtensionsTest
{
    [TestMethod]
    public void AnalyzerTargetLanguage_NotSupportedType()
    {
        var analyzerType = typeof(TypeExtensionsTest);
        var action = () => analyzerType.AnalyzerTargetLanguage();

        action.Should().Throw<NotSupportedException>().WithMessage("Can not find any language for the given type TypeExtensionsTest!");
    }

    [TestMethod]
    public void AnalyzerTargetLanguage_MultiLanguage()
    {
        var analyzerType = typeof(MultiLanguageAnalyzer);
        var action = () => analyzerType.AnalyzerTargetLanguage();

        action.Should().Throw<NotSupportedException>().WithMessage("Analyzer can not have multiple languages: MultiLanguageAnalyzer");
    }

    [TestMethod]
    public void AnalyzerTargetLanguage_SingleLanguage()
    {
        var analyzerType = typeof(SingleLanguageAnalyzer);
        var language = analyzerType.AnalyzerTargetLanguage();
        language.Should().Be(AnalyzerLanguage.CSharp);
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    private class SingleLanguageAnalyzer : SonarDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();

        protected override void Initialize(SonarAnalysisContext context) => throw new NotImplementedException();
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    private class MultiLanguageAnalyzer : SonarDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();

        protected override void Initialize(SonarAnalysisContext context) => throw new NotImplementedException();
    }
}
