/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers.Trackers
{
    public abstract class MethodDeclarationTracker<TSyntaxKind> : TrackerBase<TSyntaxKind, MethodDeclarationContext>
        where TSyntaxKind : struct
    {
        public abstract Condition ParameterAtIndexIsUsed(int index);
        protected abstract SyntaxToken? GetMethodIdentifier(SyntaxNode methodDeclaration);

        public void Track(TrackerInput input, params Condition[] conditions)
        {
            input.Context.RegisterCompilationStartAction(c =>
                {
                    if (input.IsEnabled(c.Options))
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
                        if (methodIdentifier.HasValue)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(input.Rule, methodIdentifier.Value.GetLocation()));
                        }
                    }
                }
            }

            bool IsTrackedMethod(IMethodSymbol methodSymbol, Compilation compilation)
            {
                var conditionContext = new MethodDeclarationContext(methodSymbol, compilation);
                return conditions.All(c => c.Invoke(conditionContext));
            }
        }

        public Condition MatchMethodName(params string[] methodNames) =>
            new Condition(context => methodNames.Contains(context.MethodSymbol.Name));

        public Condition IsOrdinaryMethod() =>
            new Condition(context => context.MethodSymbol.MethodKind == MethodKind.Ordinary);

        public Condition IsMainMethod() =>
            new Condition(context => context.MethodSymbol.IsMainMethod());

        internal Condition AnyParameterIsOfType(params KnownType[] types)
        {
            var typesArray = types.ToImmutableArray();
            return new Condition(context =>
                context.MethodSymbol.Parameters.Length > 0
                && context.MethodSymbol.Parameters.Any(parameter => parameter.Type.DerivesOrImplementsAny(typesArray)));
        }

        internal Condition DecoratedWithAnyAttribute(params KnownType[] attributeTypes) =>
            new Condition(context => context.MethodSymbol.GetAttributes().Any(a => a.AttributeClass.IsAny(attributeTypes)));
    }
}
