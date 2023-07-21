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

using Microsoft.CodeAnalysis.Text;

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

            void TrackMethodDeclaration(SonarSymbolReportingContext c)
            {
                if (!IsTrackedMethod((IMethodSymbol)c.Symbol, c.Compilation))
                {
                    return;
                }

                foreach (var declaration in c.Symbol.DeclaringSyntaxReferences)
                {
                    if (c.Symbol.IsTopLevelMain())
                    {
                        c.ReportIssue(Language.GeneratedCodeRecognizer, Diagnostic.Create(input.Rule, Location.Create(declaration.SyntaxTree, TextSpan.FromBounds(0, 0)).EnsureMappedLocation()));
                    }
                    else
                    {
                        var methodIdentifier = GetMethodIdentifier(declaration.GetSyntax());
                        if (methodIdentifier.HasValue)
                        {
                            c.ReportIssue(Language.GeneratedCodeRecognizer, Diagnostic.Create(input.Rule, methodIdentifier.Value.GetLocation().EnsureMappedLocation()));
                        }
                    }
                }
            }

            bool IsTrackedMethod(IMethodSymbol methodSymbol, Compilation compilation)
            {
                var conditionContext = new MethodDeclarationContext(methodSymbol, compilation);
                return conditions.All(c => c(conditionContext));
            }
        }

        public Condition MatchMethodName(params string[] methodNames) =>
            context => methodNames.Contains(context.MethodSymbol.Name);

        public Condition IsOrdinaryMethod() =>
            context => context.MethodSymbol.MethodKind == MethodKind.Ordinary;

        public Condition IsMainMethod() =>
            context => context.MethodSymbol.IsMainMethod()
                       || context.MethodSymbol.IsTopLevelMain();

        internal Condition AnyParameterIsOfType(params KnownType[] types)
        {
            var typesArray = types.ToImmutableArray();
            return context =>
                context.MethodSymbol.Parameters.Length > 0
                && context.MethodSymbol.Parameters.Any(parameter => parameter.Type.DerivesOrImplementsAny(typesArray));
        }

        internal Condition DecoratedWithAnyAttribute(params KnownType[] attributeTypes) =>
            context => context.MethodSymbol.GetAttributes().Any(a => a.AttributeClass.IsAny(attributeTypes));
    }
}
