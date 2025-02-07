/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Rules;

public abstract class AvoidDateTimeNowForBenchmarkingBase<TMemberAccess, TInvocationExpression, TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TMemberAccess : SyntaxNode
    where TInvocationExpression : SyntaxNode
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6561";
    protected override string MessageFormat => "Avoid using \"DateTime.Now\" for benchmarking or timespan calculation operations.";

    protected abstract bool ContainsDateTimeArgument(TInvocationExpression invocation, SemanticModel model);

    protected AvoidDateTimeNowForBenchmarkingBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckBinaryExpression, Language.SyntaxKind.SubtractExpression);
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckInvocation, Language.SyntaxKind.InvocationExpression);
    }

    private void CheckBinaryExpression(SonarSyntaxNodeReportingContext context)
    {
        if (Language.Syntax.BinaryExpressionLeft(context.Node) is TMemberAccess memberAccess
            && IsDateTimeNow(memberAccess, context.Model)
            && Language.Syntax.BinaryExpressionRight(context.Node) is var right
            && context.Model.GetTypeInfo(right).Type.Is(KnownType.System_DateTime))
        {
            context.ReportIssue(Rule, context.Node);
        }
    }

    private void CheckInvocation(SonarSyntaxNodeReportingContext context)
    {
        var invocation = (TInvocationExpression)context.Node;

        if (Language.Syntax.NodeExpression(invocation) is TMemberAccess subtract
            && Language.Syntax.NodeExpression(subtract) is TMemberAccess now
            && Language.Syntax.IsMemberAccessOnKnownType(subtract, "Subtract", KnownType.System_DateTime, context.Model)
            && IsDateTimeNow(now, context.Model)
            && Language.Syntax.HasExactlyNArguments(invocation, 1)
            && ContainsDateTimeArgument(invocation, context.Model))
        {
            context.ReportIssue(Rule, subtract);
        }
    }

    private bool IsDateTimeNow(TMemberAccess node, SemanticModel model) =>
        Language.Syntax.IsMemberAccessOnKnownType(node, "Now", KnownType.System_DateTime, model);
}
