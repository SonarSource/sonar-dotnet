/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class EventHandlerDelegateShouldHaveProperArguments : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4220";
        private const string MessageFormat = "{0}";
        private const string NullEventArgsMessage = "Use 'EventArgs.Empty' instead of null as the event args of " +
            "this event invocation.";
        private const string NullSenderMessage = "Make the sender on this event invocation not null.";
        private const string NonNullSenderMessage = "Make the sender on this static event invocation null.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> EventHandlerTypes =
            ImmutableArray.Create(
                KnownType.System_EventHandler,
                KnownType.System_EventHandler_TEventArgs
            );

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (invocation.ArgumentList == null ||
                        invocation.ArgumentList.Arguments.Count != 2)
                    {
                        return;
                    }

                    if (!(c.Model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol) ||
                        methodSymbol.MethodKind != MethodKind.DelegateInvoke ||
                        !methodSymbol.ContainingType.ConstructedFrom.IsAny(EventHandlerTypes))
                    {
                        return;
                    }

                    var isSecondParameterNull = invocation.ArgumentList.Arguments[1].Expression
                        .RemoveParentheses()
                        .IsNullLiteral();
                    if (isSecondParameterNull)
                    {
                        c.ReportIssue(rule, invocation, NullEventArgsMessage);
                    }

                    var eventSymbol = GetEventSymbol(invocation.Expression, c.Model);
                    if (eventSymbol == null)
                    {
                        return;
                    }

                    var isFirstParameterNull = invocation.ArgumentList.Arguments[0].Expression
                        .RemoveParentheses()
                        .IsNullLiteral();
                    if (isFirstParameterNull && !eventSymbol.IsStatic)
                    {
                        c.ReportIssue(rule, invocation, NullSenderMessage);
                    }
                    else if (!isFirstParameterNull && eventSymbol.IsStatic)
                    {
                        c.ReportIssue(rule, invocation, NonNullSenderMessage);
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
