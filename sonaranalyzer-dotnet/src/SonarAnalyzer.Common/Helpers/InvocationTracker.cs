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
    public abstract class InvocationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected InvocationTracker(IAnalyzerConfiguration analysisConfiguration, DiagnosticDescriptor rule)
            : base(analysisConfiguration, rule)
        {
        }

        protected abstract SyntaxNode GetIdentifier(SyntaxNode invocationExpression);

        protected abstract Func<MethodSignature, bool> IsSame<TSymbolType>(SyntaxNode identifier, SemanticModel semanticModel)
            where TSymbolType : class, ISymbol;

        public void Track(SonarAnalysisContext context, params MethodSignature[] methods)
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
                return identifier != null &&
                    methods.Any(IsSame<IMethodSymbol>(identifier, semanticModel));
            }
        }
    }
}
