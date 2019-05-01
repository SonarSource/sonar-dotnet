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
        private bool CaseInsensitiveComparison { get; }

        protected InvocationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule, bool caseInsensitiveComparison = false)
            : base(analyzerConfiguration, rule)
        {
            this.CaseInsensitiveComparison = caseInsensitiveComparison;
        }

        protected abstract string GetMethodName(SyntaxNode invocationExpression);

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
                var methodName = GetMethodName(invocation);
                if (methodName == null)
                {
                    return false;
                }

                var conditionContext = new InvocationContext(invocation, methodName, semanticModel);
                return conditions.All(c => c(conditionContext));
            }
        }

        public abstract InvocationCondition ArgumentAtIndexIsConstant(int index);

        public abstract InvocationCondition ArgumentAtIndexEquals(int index, string value);

        public abstract InvocationCondition IsTypeOfExpression();

        public InvocationCondition MatchMethod(params MemberDescriptor[] methods) =>
            (context) =>
                MemberDescriptor.MatchesAny(context.MethodName, context.MethodSymbol, true, CaseInsensitiveComparison, methods);

        public InvocationCondition MethodNameIs(string methodName) =>
            (context) =>
                context.MethodName == methodName;

        public InvocationCondition MethodIsStatic() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.IsStatic;

        public InvocationCondition MethodIsExtension() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.IsExtensionMethod;

        public InvocationCondition MethodHasParameters() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.Parameters.Length > 0;

        public InvocationCondition MethodHasParameters(int count) =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.Parameters.Length == count;

        internal InvocationCondition ArgumentAtIndexIs(int index, KnownType requiredType) =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.Parameters.Length > index &&
                context.MethodSymbol.Value.Parameters[index].IsType(requiredType);

        internal InvocationCondition MethodReturnTypeIs(KnownType returnType) =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.ReturnType.DerivesFrom(returnType);

        public InvocationCondition MethodIsExtern() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.IsExtern;

        #region Syntax-level checking methods

        public abstract InvocationCondition MatchProperty(MemberDescriptor member);

        #endregion
    }
}
