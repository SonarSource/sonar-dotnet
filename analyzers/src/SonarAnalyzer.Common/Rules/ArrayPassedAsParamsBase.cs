/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

public abstract class ArrayPassedAsParamsBase<TSyntaxKind, TInvocationExpressionSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TInvocationExpressionSyntax : SyntaxNode
{
    private const string DiagnosticId = "S3878";

    public const string MessageBase = "Arrays should not be created for {0} parameters.";

    private readonly DiagnosticDescriptor rule;
    protected abstract string ParameterKeyword { get; }
    protected abstract bool ShouldReport(SonarSyntaxNodeReportingContext context, TInvocationExpressionSyntax invocation);
    protected abstract Location GetLocation(TInvocationExpressionSyntax context);

    protected ArrayPassedAsParamsBase() : base(DiagnosticId)
    {
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
    }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckInvocation, Language.SyntaxKind.InvocationExpression);

    private void CheckInvocation(SonarSyntaxNodeReportingContext context)
    {
        if ((TInvocationExpressionSyntax)context.Node is var invocation
            && ShouldReport(context, invocation))
        {
            context.ReportIssue(Diagnostic.Create(rule, GetLocation(invocation), ParameterKeyword));
        }
    }
}
