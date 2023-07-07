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

namespace SonarAnalyzer.Helpers.Trackers;

public abstract class ArgumentTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, ArgumentContext>
    where TSyntaxKind : struct
{
    protected abstract RefKind? ArgumentRefKind(SyntaxNode argumentNode);
    protected abstract IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode);
    protected abstract int? Position(SyntaxNode argumentNode);
    protected abstract bool InvocationFitsMemberKind(SyntaxNode argumentNode, InvokedMemberKind memberKind);
    protected abstract bool InvokedMemberFits(SemanticModel model, SyntaxNode argumentNode, InvokedMemberKind memberKind, Func<string, bool> invokedMemberNameConstraint);
    protected abstract SyntaxNode InvokedExpression(SyntaxNode argumentNode);

    public Condition MatchArgument(ArgumentDescriptor descriptor) =>
        context =>
        {
            if (context.Node is { } argumentNode
                && SyntacticChecks(context.SemanticModel, descriptor, argumentNode)
                && InvokedExpression(argumentNode) is { } invoked
                && MethodSymbol(context.SemanticModel, invoked) is { } methodSymbol
                && Language.MethodParameterLookup(invoked, methodSymbol).TryGetSymbol(argumentNode, out var parameter)
                && ParameterFits(parameter, descriptor.ParameterConstraint, descriptor.InvokedMemberConstraint))
            {
                context.Parameter = parameter;
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

    private bool SyntacticChecks(SemanticModel model, ArgumentDescriptor descriptor, SyntaxNode argumentNode) =>
        InvocationFitsMemberKind(argumentNode, descriptor.MemberKind)
        && (descriptor.RefKind is null || (ArgumentRefKind(argumentNode) is { } refKind ? refKind == descriptor.RefKind.Value : true))
        && (descriptor.ArgumentListConstraint == null
            || (ArgumentList(argumentNode) is { } argList && descriptor.ArgumentListConstraint(argList, Position(argumentNode))))
        && (descriptor.InvokedMemberNameConstraint == null
            || InvokedMemberFits(model, argumentNode, descriptor.MemberKind, x => descriptor.InvokedMemberNameConstraint(x, Language.NameComparison)));

    private static bool ParameterFits(IParameterSymbol parameter, Func<IParameterSymbol, bool> parameterConstraint, Func<IMethodSymbol, bool> invokedMemberConstraint)
    {
        if ((parameter.ContainingSymbol is IMethodSymbol method)
            && method.Parameters.IndexOf(parameter) is >= 0 and int position)
        {
            do
            {
                if (invokedMemberConstraint?.Invoke(method) is null or true && parameterConstraint?.Invoke(method.Parameters[position]) is null or true)
                {
                    return true;
                }
            }
            while ((method = method.OverriddenMethod) != null);
        }
        return false;
    }
}
