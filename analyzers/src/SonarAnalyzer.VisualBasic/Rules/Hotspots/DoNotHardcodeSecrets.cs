/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class DoNotHardcodeSecrets : DoNotHardcodeSecretsBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    public DoNotHardcodeSecrets() : this(AnalyzerConfiguration.Hotspot) { }

    public DoNotHardcodeSecrets(IAnalyzerConfiguration configuration) : base(configuration) { }

    protected override void RegisterNodeActions(SonarCompilationStartAnalysisContext context)
    {
        context.RegisterNodeAction(
            ReportIssues,
            SyntaxKind.AddAssignmentStatement,
            SyntaxKind.ConcatenateAssignmentStatement,
            SyntaxKind.SimpleAssignmentStatement,
            SyntaxKind.VariableDeclarator,
            SyntaxKind.PropertyStatement,
            SyntaxKind.EqualsExpression);

        context.RegisterNodeAction(c =>
            {
                var invocationExpression = (InvocationExpressionSyntax)c.Node;

                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression
                    && memberAccessExpression.Name.Identifier.ValueText == EqualsName
                    && invocationExpression.ArgumentList?.Arguments.FirstOrDefault() is { } firstArgument
                    && memberAccessExpression.IsMemberAccessOnKnownType(EqualsName, KnownType.System_String, c.Model))
                {
                    ReportIssuesForEquals(c, memberAccessExpression, IdentifierAndValue(memberAccessExpression.Expression, firstArgument));
                }
            },
            SyntaxKind.InvocationExpression);
    }

    protected override SyntaxNode IdentifierRoot(SyntaxNode node) =>
        node switch
        {
            AssignmentStatementSyntax assignment => assignment.Left,
            BinaryExpressionSyntax { Left: IdentifierNameSyntax left } => left,
            BinaryExpressionSyntax { Right: IdentifierNameSyntax right } => right,
            _ => node
        };

    protected override SyntaxNode RightHandSide(SyntaxNode node) =>
        node switch
        {
            AssignmentStatementSyntax assignment => assignment.Right,
            VariableDeclaratorSyntax variable => variable.Initializer?.Value,
            PropertyStatementSyntax property => property.Initializer?.Value,
            BinaryExpressionSyntax { Left: IdentifierNameSyntax } binary => binary.Right,
            BinaryExpressionSyntax { Right: IdentifierNameSyntax } binary => binary.Left,
            _ => null
        };

    private static IdentifierValuePair IdentifierAndValue(ExpressionSyntax expression, ArgumentSyntax argument) =>
        expression switch
        {
            MemberAccessExpressionSyntax or IdentifierNameSyntax or InvocationExpressionSyntax => new(expression.GetIdentifier(), argument.GetExpression()),
            LiteralExpressionSyntax literal => new(argument.GetExpression().GetIdentifier(), literal),
            _ => null,
        };
}
