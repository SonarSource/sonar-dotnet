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

namespace SonarAnalyzer.CSharp.Core.Trackers;

public class CSharpElementAccessTracker : ElementAccessTracker<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override SyntaxKind[] TrackedSyntaxKinds { get; } = [SyntaxKind.ElementAccessExpression, SyntaxKind.ElementBindingExpression];

    public override object AssignedValue(ElementAccessContext context) =>
        context.Node.Ancestors().FirstOrDefault(x => x.IsKind(SyntaxKind.SimpleAssignmentExpression)) is AssignmentExpressionSyntax assignment
            ? assignment.Right.FindConstantValue(context.Model)
            : null;

    public override Condition ArgumentAtIndexEquals(int index, string value)
    {
        return context => ArgumentList(context.Node) is { } arguments
                            && index < arguments.Count
                            && arguments[index].Expression.FindStringConstant(context.Model) == value;

        static SeparatedSyntaxList<ArgumentSyntax>? ArgumentList(SyntaxNode node) => node switch
        {
            ElementAccessExpressionSyntax elementAccess => elementAccess.ArgumentList.Arguments,
            ElementBindingExpressionSyntax elementBinding => elementBinding.ArgumentList.Arguments,
            _ => null
        };
    }

    public override Condition MatchSetter() =>
        context => ((ExpressionSyntax)context.Node).IsLeftSideOfAssignment();

    public override Condition MatchProperty(MemberDescriptor member) =>
        context => context.Node switch
        {
            ElementAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax { RawKind: (int)SyntaxKind.SimpleMemberAccessExpression } memberAccessExpression } =>
                member.IsMatch(memberAccessExpression.Name.Identifier.ValueText, context.Model.GetTypeInfo(memberAccessExpression.Expression).Type, Language.NameComparison),
            ElementAccessExpressionSyntax { Expression: MemberBindingExpressionSyntax memberBindingExpression } =>
                member.IsMatch(memberBindingExpression.Name.Identifier.ValueText, context.Model.GetSymbolInfo(memberBindingExpression).Symbol?.ContainingType, Language.NameComparison),
            ElementBindingExpressionSyntax { Parent.Parent: ConditionalAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccessExpression } } =>
                member.IsMatch(memberAccessExpression.Name.Identifier.ValueText, context.Model.GetTypeInfo(memberAccessExpression.Expression).Type, Language.NameComparison),
            _ => false
        };
}
