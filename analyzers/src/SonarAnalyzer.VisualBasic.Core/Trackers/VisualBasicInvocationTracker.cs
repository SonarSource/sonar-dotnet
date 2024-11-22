/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

public class VisualBasicInvocationTracker : InvocationTracker<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
    protected override SyntaxKind[] TrackedSyntaxKinds { get; } = { SyntaxKind.InvocationExpression };

    public override Condition ArgumentAtIndexIsStringConstant(int index) =>
        ArgumentAtIndexConformsTo(index, (argument, model) =>
            argument.GetExpression().FindStringConstant(model) is not null);

    public override Condition ArgumentAtIndexIsAny(int index, params string[] values) =>
        ArgumentAtIndexConformsTo(index, (argument, model) =>
            values.Contains(argument.GetExpression().FindStringConstant(model)));

    public override Condition ArgumentAtIndexIs(int index, Func<SyntaxNode, SemanticModel, bool> predicate) =>
        ArgumentAtIndexConformsTo(index, (argument, model) =>
            predicate(argument, model));

    public override Condition MatchProperty(MemberDescriptor member) =>
        context => ((InvocationExpressionSyntax)context.Node).Expression is MemberAccessExpressionSyntax methodMemberAccess
                   && methodMemberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                   && methodMemberAccess.Expression is MemberAccessExpressionSyntax propertyMemberAccess
                   && propertyMemberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                   && context.Model.GetTypeInfo(propertyMemberAccess.Expression) is TypeInfo enclosingClassType
                   && member.IsMatch(propertyMemberAccess.Name.Identifier.ValueText, enclosingClassType.Type, Language.NameComparison);

    public override object ConstArgumentForParameter(InvocationContext context, string parameterName)
    {
        var argumentList = ((InvocationExpressionSyntax)context.Node).ArgumentList;
        var values = argumentList.ArgumentValuesForParameter(context.Model, parameterName);
        return values.Length == 1 && values[0] is ExpressionSyntax valueSyntax
            ? valueSyntax.FindConstantValue(context.Model)
            : null;
    }

    protected override SyntaxToken? ExpectedExpressionIdentifier(SyntaxNode expression) =>
        ((ExpressionSyntax)expression).GetIdentifier();

    private static Condition ArgumentAtIndexConformsTo(int index, Func<ArgumentSyntax, SemanticModel, bool> predicate) =>
        context => context.Node is InvocationExpressionSyntax { ArgumentList: { } argumentList }
            && index < argumentList.Arguments.Count
            && argumentList.Arguments[index] is { } argument
            && predicate(argument, context.Model);
}
