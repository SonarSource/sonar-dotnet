/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Trackers;

public class CSharpInvocationTracker : InvocationTracker<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
    protected override SyntaxKind[] TrackedSyntaxKinds { get; } = new[] { SyntaxKind.InvocationExpression };

    public override Condition ArgumentAtIndexIsStringConstant(int index) =>
        ArgumentAtIndexConformsTo(index, (argument, model) =>
            argument.Expression.FindStringConstant(model) is not null);

    public override Condition ArgumentAtIndexIsAny(int index, params string[] values) =>
        ArgumentAtIndexConformsTo(index, (argument, model) =>
            values.Contains(argument.Expression.FindStringConstant(model)));

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
        context => context.Node is InvocationExpressionSyntax { ArgumentList.Arguments: { } arguments }
            && index < arguments.Count
            && arguments[index] is { } argument
            && predicate(argument, context.Model);
}
