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

namespace SonarAnalyzer.Rules;

public abstract class ArrayPassedAsParamsBase<TSyntaxKind, TInvocationExpressionSyntax, TObjectCreationExpressionSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpressionSyntax : SyntaxNode
    where TObjectCreationExpressionSyntax : SyntaxNode
{
    private const string DiagnosticId = "S3878";

    public static readonly string MessageBase = "Arrays should not be created for {0} parameters.";

    private readonly DiagnosticDescriptor rule;
    protected abstract string ParameterKeyword { get; }
    protected abstract bool ShouldReportInvocation(TInvocationExpressionSyntax invocation);
    protected abstract bool ShouldReportCreation(TObjectCreationExpressionSyntax creation);
    protected abstract Location GetInvocationLocation(TInvocationExpressionSyntax context);
    protected abstract Location GetCreationLocation(TObjectCreationExpressionSyntax context);

    protected ArrayPassedAsParamsBase() : base(DiagnosticId) =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckInvocation, Language.SyntaxKind.InvocationExpression);
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckObjectCreation, Language.SyntaxKind.ObjectCreationExpressions);
    }

    private void CheckInvocation(SonarSyntaxNodeReportingContext context)
    {
        if (context.Node is TInvocationExpressionSyntax invocation
            && ShouldReportInvocation(invocation)
            && context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedMethodSymbol
            && invokedMethodSymbol.Parameters.Any()
            && invokedMethodSymbol.Parameters.Last().IsParams) // params keyword should be only one and no additional parameters are permitted after that.
        {
            Report(context, GetInvocationLocation(invocation));
        }
    }

    private void CheckObjectCreation(SonarSyntaxNodeReportingContext context)
    {
        if (context.Node is TObjectCreationExpressionSyntax creation
            && ShouldReportCreation(creation)
            && context.SemanticModel.GetSymbolInfo(creation).Symbol is IMethodSymbol invokedMethodSymbol
            && invokedMethodSymbol.Parameters.Any()
            && invokedMethodSymbol.Parameters.Last().IsParams) // params keyword should be only one and no additional parameters are permitted after that.
        {
            Report(context, GetCreationLocation(creation));
        }
    }

    private void Report(SonarSyntaxNodeReportingContext context, Location location) =>
        context.ReportIssue(Diagnostic.Create(rule, location, ParameterKeyword));
}
