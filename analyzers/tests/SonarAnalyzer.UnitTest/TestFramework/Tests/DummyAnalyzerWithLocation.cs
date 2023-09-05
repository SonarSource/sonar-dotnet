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
internal class DummyAnalyzerWithLocation : SonarDiagnosticAnalyzer
{
    private readonly DiagnosticDescriptor Rule;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public DummyAnalyzerWithLocation() : this("DummyWithLocation", false) { }

    public DummyAnalyzerWithLocation(string id, bool isTest) =>
        Rule = isTest ? AnalysisScaffolding.CreateDescriptorTest(id) : AnalysisScaffolding.CreateDescriptorMain(id);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(CSharpGeneratedCodeRecognizer.Instance, ReportIssue, SyntaxKind.InvocationExpression);

    private void ReportIssue(SonarSyntaxNodeReportingContext context)
    {
        if (context.Node is InvocationExpressionSyntax invocation
            && invocation.NodeIdentifier() is { ValueText: "RaiseHere" } identifier)
        {
            context.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation(), invocation.ArgumentList.Arguments.Select(arg => arg.GetLocation())));
        }
    }
}
