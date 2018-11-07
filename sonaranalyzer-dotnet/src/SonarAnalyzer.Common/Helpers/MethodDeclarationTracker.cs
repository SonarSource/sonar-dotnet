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
    public delegate bool MethodDeclarationCondition(MethodDeclarationContext context);

    public abstract class MethodDeclarationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected MethodDeclarationTracker(IAnalyzerConfiguration analysisConfiguration, DiagnosticDescriptor rule)
            : base(analysisConfiguration, rule)
        {
        }

        protected abstract SyntaxToken GetMethodIdentifier(SyntaxNode methodDeclaration);

        public void Track(SonarAnalysisContext context, params MethodDeclarationCondition[] conditions)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSymbolAction(TrackMethodDeclaration, SymbolKind.Method);
                    }
                });

            void TrackMethodDeclaration(SymbolAnalysisContext c)
            {
                if (IsTrackedMethod((IMethodSymbol)c.Symbol, c.Compilation))
                {
                    foreach (var declaration in c.Symbol.DeclaringSyntaxReferences)
                    {
                        var methodIdentifier = GetMethodIdentifier(declaration.GetSyntax());
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, methodIdentifier.GetLocation()));
                    }
                }
            }

            bool IsTrackedMethod(IMethodSymbol methodSymbol, Compilation compilation)
            {
                var conditionContext = new MethodDeclarationContext(methodSymbol, compilation);
                return conditions.All(c => c(conditionContext));
            }
        }

        public abstract MethodDeclarationCondition ParameterAtIndexIsUsed(int index);

        public MethodDeclarationCondition MatchSimpleNames(params string[] methodNames) =>
            (context) =>
                methodNames.Contains(context.MethodSymbol.Name);

        public MethodDeclarationCondition IsMainMethod() =>
            (context) =>
                context.MethodSymbol.IsMainMethod();
    }
}
