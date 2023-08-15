/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2023 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
internal class DummyAnalyzerWithLocationMapping : SonarDiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = AnalysisScaffolding.CreateDescriptorMain("DummyWithLocation");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(CSharpGeneratedCodeRecognizer.Instance, ReportIssue, SyntaxKind.InvocationExpression);

    private void ReportIssue(SonarSyntaxNodeReportingContext context)
    {
        if (context.Node is InvocationExpressionSyntax invocation
            && invocation.NodeIdentifier() is { } identifier
            && identifier.ValueText == "RaiseHere")
        {
            var location = identifier.GetLocation();
            var lineSpan = location.GetMappedLineSpan();
            location = Location.Create(lineSpan.Path, location.SourceSpan, lineSpan.Span);
            context.ReportIssue(Diagnostic.Create(Rule, location));
        }
    }
}
