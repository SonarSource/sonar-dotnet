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

namespace SonarAnalyzer.UnitTest.TestFramework.Tests;

internal abstract class TestAnalyzer : SonarDiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = AnalysisScaffolding.CreateDescriptorMain("SDummy");
    protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
}

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class TestAnalyzerCS : TestAnalyzer
{
    private readonly Action<SonarAnalysisContext, GeneratedCodeRecognizer> initializeAction;
    protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;

    public TestAnalyzerCS(Action<SonarAnalysisContext, GeneratedCodeRecognizer> action) =>
        initializeAction = action;

    protected override void Initialize(SonarAnalysisContext context) =>
        initializeAction(context, GeneratedCodeRecognizer);
}

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
internal class TestAnalyzerVB : TestAnalyzer
{
    private readonly Action<SonarAnalysisContext, GeneratedCodeRecognizer> initializeAction;
    protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;

    public TestAnalyzerVB(Action<SonarAnalysisContext, GeneratedCodeRecognizer> action) =>
        initializeAction = action;

    protected override void Initialize(SonarAnalysisContext context) =>
        initializeAction(context, GeneratedCodeRecognizer);
}
