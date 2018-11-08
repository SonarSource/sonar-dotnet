/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public delegate bool InvocationCondition(InvocationContext invocationContext);

    public abstract class InvocationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected InvocationTracker(IAnalyzerConfiguration analysisConfiguration, DiagnosticDescriptor rule)
            : base(analysisConfiguration, rule)
        {
        }

        protected abstract SyntaxNode GetIdentifier(SyntaxNode invocationExpression);

        public void Track(SonarAnalysisContext context, params InvocationCondition[] conditions)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSyntaxNodeActionInNonGenerated(
                            GeneratedCodeRecognizer,
                            TrackInvocationExpression,
                            TrackedSyntaxKinds);
                    }
                });

            void TrackInvocationExpression(SyntaxNodeAnalysisContext c)
            {
                if (IsTrackedMethod(c.Node, c.SemanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            }

            bool IsTrackedMethod(SyntaxNode invocation, SemanticModel semanticModel)
            {
                var identifier = GetIdentifier(invocation);
                if (identifier == null)
                {
                    return false;
                }

                var conditionContext = new InvocationContext(invocation, identifier, semanticModel);
                return conditions.All(c => c(conditionContext));
            }
        }

        #region Syntax-level standard conditions

        public abstract InvocationCondition MatchSimpleNames(params MethodSignature[] methods);

        public abstract InvocationCondition FirstParameterIsConstant();

        #endregion

        #region Symbol-level standard conditions

        public InvocationCondition IsStatic() =>
            (context) =>
                context.InvokedMethodSymbol.Value != null &&
                context.InvokedMethodSymbol.Value.IsStatic;

        public bool FirstParameterIsString(InvocationContext context)
        {
            var firstParam = context.InvokedMethodSymbol.Value?.Parameters.FirstOrDefault();

            return firstParam != null &&
                firstParam.IsType(KnownType.System_String);
        }

        internal InvocationCondition FirstParameterIsOfType(KnownType requiredType)
        {
            return Check;

            bool Check(InvocationContext context) =>
                context.InvokedMethodSymbol.Value != null &&
                context.InvokedMethodSymbol.Value.Parameters.Length > 0 &&
                context.InvokedMethodSymbol.Value.Parameters[0].IsType(requiredType);
        }

        #endregion
    }
}
