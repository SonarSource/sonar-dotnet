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
        protected InvocationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule)
            : base(analyzerConfiguration, rule)
        {
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

        #region Syntax-level standard conditions

        public InvocationCondition FirstParameterIsConstant() =>
            ArgumentAtIndexIsConstant(0);

        public abstract InvocationCondition ArgumentAtIndexIsConstant(int index);

        public abstract InvocationCondition ArgumentAtIndexIsString(int index, string value);

        public abstract InvocationCondition IsTypeOfExpression();

        #endregion

        #region Symbol-level standard conditions

        public InvocationCondition MatchSimpleNames(params MemberDescriptor[] methods) =>
            (context) =>
                MemberDescriptorHelper.IsMatch(context.MethodName, context.MethodSymbol, true, methods);

        public InvocationCondition MethodNameIs(string methodName) =>
            (context) =>
                context.MethodName == methodName;

        public InvocationCondition IsStatic() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.IsStatic;

        public InvocationCondition IsExtensionMethod() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.IsExtensionMethod;

        public InvocationCondition HasParameters() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.Parameters.Length > 0;

        public InvocationCondition HasParameters(int count) =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.Parameters.Length == count;

        public bool FirstParameterIsString(InvocationContext context)
        {
            var firstParam = context.MethodSymbol.Value?.Parameters.FirstOrDefault();
            return firstParam != null &&
                firstParam.IsType(KnownType.System_String);
        }

        internal InvocationCondition FirstParameterIsOfType(KnownType requiredType) =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.Parameters.Length > 0 &&
                context.MethodSymbol.Value.Parameters[0].IsType(requiredType);

        internal InvocationCondition WhenReturnTypeIs(KnownType returnType) =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.ReturnType.DerivesFrom(returnType);

        public InvocationCondition IsExtern() =>
            (context) =>
                context.MethodSymbol.Value != null &&
                context.MethodSymbol.Value.IsExtern;

        #endregion
    }
}
