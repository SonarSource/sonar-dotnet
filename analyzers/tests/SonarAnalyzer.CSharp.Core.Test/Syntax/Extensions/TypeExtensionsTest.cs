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

using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Core.Analyzers;

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
