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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Trackers;

public class VisualBasicElementAccessTracker : ElementAccessTracker<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
    protected override SyntaxKind[] TrackedSyntaxKinds { get; } = { SyntaxKind.InvocationExpression };

    public override object AssignedValue(ElementAccessContext context) =>
        context.Node.Ancestors().FirstOrDefault(x => x.IsKind(SyntaxKind.SimpleAssignmentStatement)) is AssignmentStatementSyntax assignment
            ? assignment.Right.FindConstantValue(context.Model)
            : null;

    public override Condition ArgumentAtIndexEquals(int index, string value) =>
        context => ((InvocationExpressionSyntax)context.Node).ArgumentList is { } argumentList
                   && index < argumentList.Arguments.Count
                   && argumentList.Arguments[index].GetExpression().FindStringConstant(context.Model) == value;

    public override Condition MatchSetter() =>
        context => ((ExpressionSyntax)context.Node).IsLeftSideOfAssignment();

    public override Condition MatchProperty(MemberDescriptor member) =>
        context => ((InvocationExpressionSyntax)context.Node).Expression is MemberAccessExpressionSyntax memberAccess
                   && memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                   && context.Model.GetTypeInfo(memberAccess.Expression) is TypeInfo enclosingClassType
                   && member.IsMatch(memberAccess.Name.Identifier.ValueText, enclosingClassType.Type, Language.NameComparison);
}
