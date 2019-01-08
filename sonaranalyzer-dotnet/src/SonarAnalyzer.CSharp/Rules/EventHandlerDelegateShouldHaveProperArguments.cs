/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EventHandlerDelegateShouldHaveProperArguments : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4220";
        private const string MessageFormat = "{0}";
        private const string NullEventArgsMessage = "Use 'EventArgs.Empty' instead of null as the event args of " +
            "this event invocation.";
        private const string NullSenderMessage = "Make the sender on this event invocation not null.";
        private const string NonNullSenderMessage = "Make the sender on this static event invocation null.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> EventHandlerTypes =
            ImmutableArray.Create(
                KnownType.System_EventHandler,
                KnownType.System_EventHandler_TEventArgs
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (invocation.ArgumentList == null ||
                        invocation.ArgumentList.Arguments.Count != 2)
                    {
                        return;
                    }

                    if (!(c.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol) ||
                        methodSymbol.MethodKind != MethodKind.DelegateInvoke ||
                        !methodSymbol.ContainingType.ConstructedFrom.IsAny(EventHandlerTypes))
                    {
                        return;
                    }

                    var isSecondParameterNull = invocation.ArgumentList.Arguments[1].Expression
                        .RemoveParentheses()
                        .IsKind(SyntaxKind.NullLiteralExpression);
                    if (isSecondParameterNull)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation(), NullEventArgsMessage));
                    }

                    var eventSymbol = GetEventSymbol(invocation.Expression, c.SemanticModel);
                    if (eventSymbol == null)
                    {
                        return;
                    }

                    var isFirstParameterNull = invocation.ArgumentList.Arguments[0].Expression
                        .RemoveParentheses()
                        .IsKind(SyntaxKind.NullLiteralExpression);
                    if (isFirstParameterNull && !eventSymbol.IsStatic)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation(), NullSenderMessage));
                    }
                    else if (!isFirstParameterNull && eventSymbol.IsStatic)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, invocation.GetLocation(), NonNullSenderMessage));
                    }
                },
                SyntaxKind.InvocationExpression);
        }

        private static IEventSymbol GetEventSymbol(ExpressionSyntax expression, SemanticModel model)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.IsKind(SyntaxKind.IdentifierName))
            {
                return model.GetSymbolInfo(expression).Symbol as IEventSymbol;
            }

            if (expression is MemberAccessExpressionSyntax simpleMemberAccess)
            {
                return GetEventSymbol(simpleMemberAccess.Expression, model);
            }

            if (expression is MemberBindingExpressionSyntax)
            {
                var conditionalExpression = expression.FirstAncestorOrSelf<ConditionalAccessExpressionSyntax>();
                var isSelf = expression == conditionalExpression?.Expression;

                if (isSelf)
                {
                    return null;
                }

                return GetEventSymbol(conditionalExpression?.Expression, model);
            }

            return null;
        }
    }
}
