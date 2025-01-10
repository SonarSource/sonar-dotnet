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

namespace SonarAnalyzer.Rules;

public abstract class ToStringShouldNotReturnNullBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2225";

    protected override string MessageFormat => "Return an empty string instead.";

    protected abstract TSyntaxKind MethodKind { get; }

    protected abstract bool IsLocalOrLambda(SyntaxNode node);

    protected abstract IEnumerable<SyntaxNode> Conditionals(SyntaxNode expression);

    protected ToStringShouldNotReturnNullBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c => ToStringReturnsNull(c, c.Node),
            Language.SyntaxKind.ReturnStatement);

    protected void ToStringReturnsNull(SonarSyntaxNodeReportingContext context, SyntaxNode node)
    {
        if (node is not null && ReturnsNull(Language.Syntax.NodeExpression(node)) && WithinToString(node))
        {
            context.ReportIssue(Rule, node);
        }
    }

    private bool ReturnsNull(SyntaxNode node) =>
        Language.Syntax.IsNullLiteral(node)
        || Conditionals(node).Select(Language.Syntax.RemoveParentheses).Any(ReturnsNull);

    private bool WithinToString(SyntaxNode node) =>
        node.Ancestors()
            .TakeWhile(x => !IsLocalOrLambda(x))
            .Any(x => Language.Syntax.IsKind(x, MethodKind)
                && nameof(ToString).Equals(Language.Syntax.NodeIdentifier(x)?.ValueText, Language.NameComparison)
                && !Language.Syntax.IsStatic(x));
}
