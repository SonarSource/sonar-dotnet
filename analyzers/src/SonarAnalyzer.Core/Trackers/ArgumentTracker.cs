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

namespace SonarAnalyzer.Core.Trackers;

public abstract class ArgumentTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, ArgumentContext>
    where TSyntaxKind : struct
{
    protected abstract RefKind? ArgumentRefKind(SyntaxNode argumentNode);
    protected abstract IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode);
    protected abstract int? Position(SyntaxNode argumentNode);
    protected abstract bool InvocationMatchesMemberKind(SyntaxNode invokedExpression, MemberKind memberKind);
    protected abstract bool InvokedMemberMatches(SemanticModel model, SyntaxNode invokedExpression, MemberKind memberKind, Func<string, bool> invokedMemberNameConstraint);

    protected override ArgumentContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        new(context);

    public Condition MatchArgument(ArgumentDescriptor descriptor) =>
        trackingContext =>
        {
            if (trackingContext.Node is { } argumentNode
                && argumentNode is { Parent.Parent: { } invoked }
                && SyntacticChecks(trackingContext.SemanticModel, descriptor, argumentNode, invoked)
                && (descriptor.InvokedMemberNodeConstraint?.Invoke(trackingContext.SemanticModel, Language, invoked) ?? true)
                && MethodSymbol(trackingContext.SemanticModel, invoked) is { } methodSymbol
                && Language.MethodParameterLookup(invoked, methodSymbol).TryGetSymbol(argumentNode, out var parameter)
                && ParameterMatches(parameter, descriptor.ParameterConstraint, descriptor.InvokedMemberConstraint))
            {
                trackingContext.Parameter = parameter;
                return true;
            }
            return false;
        };

    private IMethodSymbol MethodSymbol(SemanticModel model, SyntaxNode invoked) =>
        model.GetSymbolInfo(invoked).Symbol switch
        {
            IMethodSymbol x => x,
            IPropertySymbol propertySymbol => Language.Syntax.IsWrittenTo(invoked, model, CancellationToken.None)
                ? propertySymbol.SetMethod
                : propertySymbol.GetMethod,
            _ => null,
        };

    // SemanticModel is needed for target-typed-new only.
    private bool SyntacticChecks(SemanticModel model, ArgumentDescriptor descriptor, SyntaxNode argumentNode, SyntaxNode invokedExpression) =>
        InvocationMatchesMemberKind(invokedExpression, descriptor.MemberKind)
        && RefKindMatches(descriptor, argumentNode)
        && (descriptor.ArgumentListConstraint is null
            || (ArgumentList(argumentNode) is { } argList && descriptor.ArgumentListConstraint(argList, Position(argumentNode))))
        && (descriptor.InvokedMemberNameConstraint is null
            || InvokedMemberMatches(model, invokedExpression, descriptor.MemberKind, x => descriptor.InvokedMemberNameConstraint(x, Language.NameComparison)));

    private bool RefKindMatches(ArgumentDescriptor descriptor, SyntaxNode argumentNode) =>
        descriptor.RefKind is not { } expectedRefKind // When null: No RefKind constraint was specified
        || ArgumentRefKind(argumentNode) is not { } actualRefKind // When null: In VB, the argument does not need ref/out keywords on the call side
        || expectedRefKind == actualRefKind
        // For parameter ref kind "in", on the call side "in" is optional or can be "ref" instead
        // For "ref readonly" parameters, "none", "in" and "ref" are allowed on the call side
        || (expectedRefKind is RefKindEx.In && actualRefKind is RefKind.None or RefKind.Ref)
        || (expectedRefKind is RefKindEx.RefReadOnlyParameter && actualRefKind is RefKind.None or RefKind.Ref or RefKindEx.In);

    private static bool ParameterMatches(IParameterSymbol parameter, Func<IParameterSymbol, bool> parameterConstraint, Func<IMethodSymbol, bool> invokedMemberConstraint)
    {
        if (parameter.ContainingSymbol is IMethodSymbol method
            && method.Parameters.IndexOf(parameter) is >= 0 and int position)
        {
            do
            {
                if (invokedMemberConstraint?.Invoke(method) is null or true && parameterConstraint?.Invoke(method.Parameters[position]) is null or true)
                {
                    return true;
                }
            }
            while ((method = method.OverriddenMethod) is not null);
        }
        return false;
    }
}
