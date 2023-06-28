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

public abstract class ArgumentTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, SyntaxBaseContext>
    where TSyntaxKind : struct
{
    protected abstract RefKind ArgumentRefKind(SyntaxNode argumentNode);
    protected abstract IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode);
    protected abstract int? Position(SyntaxNode argumentNode);
    protected abstract bool InvocationFitsMemberKind(SyntaxNode argumentNode, InvokedMemberKind memberKind);
    protected abstract bool InvokedMemberFits(SemanticModel model, SyntaxNode argumentNode, InvokedMemberKind memberKind, Func<string, bool> invokedMemberNameConstraint);
    protected abstract SyntaxNode InvokedExpression(SyntaxNode argumentNode);

    public Condition MatchArgument(ArgumentDescriptor argument) =>
        context =>
        {
            var argumentNode = context.Node;
            if (argument.RefKind is null || ArgumentRefKind(argumentNode) == argument.RefKind.Value)
            {
                var argList = ArgumentList(argumentNode);
                if (argList == null)
                {
                    return false;
                }
                if (argument.ArgumentListConstraint?.Invoke(argList, Position(argumentNode)) is null or true)
                {
                    if (InvocationFitsMemberKind(argumentNode, argument.MemberKind))
                    {
                        if (argument.InvokedMemberNameConstraint != null
                            && this.InvokedMemberFits(context.SemanticModel, argumentNode, argument.MemberKind, x => argument.InvokedMemberNameConstraint(x, Language.NameComparison)))
                        {
                            var invoked = InvokedExpression(argumentNode);
                            var symbol = context.SemanticModel.GetSymbolInfo(invoked).Symbol;
                            var methodSymbol = symbol switch
                            {
                                IMethodSymbol x => x,
                                IPropertySymbol propertySymbol => Language.Syntax.IsWrittenTo(invoked, context.SemanticModel, CancellationToken.None)
                                    ? propertySymbol.SetMethod
                                    : propertySymbol.GetMethod,
                                _ => null,
                            };
                            var parameterLookup = Language.MethodParameterLookup(invoked, methodSymbol);
                            if (parameterLookup.TryGetSymbol(argumentNode, out var parameter))
                            {
                                return ParameterFits(parameter, argument.ParameterConstraint, argument.InvokedMemberConstraint);
                            }
                        }
                    }
                }
            }
            return false;
        };

    private static bool ParameterFits(IParameterSymbol parameter, Func<IParameterSymbol, bool> parameterConstraint, Func<ISymbol, bool> invokedMemberConstraint)
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
