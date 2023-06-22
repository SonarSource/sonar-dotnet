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

public abstract class AvoidDateTimeNowForBenchmarkingBase<TMemberAccess, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TMemberAccess : SyntaxNode
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6561";
    protected override string MessageFormat => "Avoid using \"DateTime.Now\" for benchmarking or timing operations";
    protected abstract SyntaxNode GetExpression(TMemberAccess memberAccess);

    protected AvoidDateTimeNowForBenchmarkingBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckBinaryExpression, Language.SyntaxKind.SubtractExpression);
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckMemberAccess, Language.SyntaxKind.SimpleMemberAccessExpression);
    }

    private void CheckBinaryExpression(SonarSyntaxNodeReportingContext context)
    {
        if (Language.Syntax.BinaryExpressionLeft(context.Node) is TMemberAccess memberAccess
            && IsDateTimeNow(memberAccess, context.SemanticModel)
            && Language.Syntax.BinaryExpressionRight(context.Node) is var right
            && context.SemanticModel.GetTypeInfo(right).Type.Is(KnownType.System_DateTime))
        {
            context.ReportIssue(Diagnostic.Create(Rule, context.Node.GetLocation()));
        }
    }

    private void CheckMemberAccess(SonarSyntaxNodeReportingContext context)
    {
        var memberAccess = (TMemberAccess)context.Node;

        if (IsDateTimeSubtract(memberAccess, context.SemanticModel)
            && GetExpression(memberAccess) is TMemberAccess childMemberAccess
            && IsDateTimeNow(childMemberAccess, context.SemanticModel)
            && context.SemanticModel.GetSymbolInfo(memberAccess).Symbol is IMethodSymbol methodSymbol
            && methodSymbol.Parameters.First().IsType(KnownType.System_DateTime))
        {
            context.ReportIssue(Diagnostic.Create(Rule, memberAccess.GetLocation()));
        }
    }

    private bool IsDateTimeNow(TMemberAccess node, SemanticModel model) =>
        Language.Syntax.IsMemberAccessOnKnownType(node, "Now", KnownType.System_DateTime, model);

    private bool IsDateTimeSubtract(TMemberAccess node, SemanticModel model) =>
        Language.Syntax.IsMemberAccessOnKnownType(node, "Subtract", KnownType.System_DateTime, model);
}
