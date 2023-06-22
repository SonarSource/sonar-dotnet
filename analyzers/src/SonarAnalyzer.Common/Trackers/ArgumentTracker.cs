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

namespace SonarAnalyzer.Trackers;

public abstract class ArgumentTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, SyntaxBaseContext>
    where TSyntaxKind : struct
{
    protected abstract RefKind ArgumentRefKind(SyntaxNode argumentNode);
    protected abstract IReadOnlyCollection<SyntaxNode> ArgumentList(SyntaxNode argumentNode);
    protected abstract bool InvocationFitsMemberKind(SyntaxNode argumentNode, InvokedMemberKind memberKind);
    protected abstract bool InvokedMemberFits(SemanticModel model, SyntaxNode argumentNode, InvokedMemberKind memberKind, Func<string, bool> invokedMemberNameConstraint);
    protected abstract SyntaxNode InvokedExpression(SyntaxNode argumentNode);

    public Condition MatchArgument(ArgumentDescriptor argument) =>
        context =>
        {
            if (argument.RefKind is { } refKind && ArgumentRefKind(context.Node) == refKind)
            {
                var argList = ArgumentList(context.Node);
                if (argList == null)
                {
                    return false;
                }
                if (argument.ArgumentListConstraint?.Invoke(argList) is null or true)
                {
                    if (InvocationFitsMemberKind(context.Node, argument.MemberKind))
                    {
                        if (argument.InvokedMemberNameConstraint != null
                            && InvokedMemberFits(context.SemanticModel, context.Node, argument.MemberKind, x => argument.InvokedMemberNameConstraint(x, Language.NameComparison)))
                        {
                            var parameterLookup = Language.MethodParameterLookup(InvokedExpression(context.Node), context.SemanticModel);
                            if (parameterLookup.TryGetSymbol(context.Node, out var parameter))
                            {
                                if (argument.ParameterConstraint?.Invoke(parameter) is null or true)
                                {
                                    return argument.InvokedMemberConstraint?.Invoke(parameter.ContainingSymbol) is null or true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        };
}
