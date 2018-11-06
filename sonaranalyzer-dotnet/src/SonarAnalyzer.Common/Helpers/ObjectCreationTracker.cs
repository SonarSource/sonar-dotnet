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
    public delegate bool ObjectCreationCondition(ObjectCreationContext context);

    public abstract class ObjectCreationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected ObjectCreationTracker(IAnalyzerConfiguration analysisConfiguration, DiagnosticDescriptor rule)
            : base(analysisConfiguration, rule)
        {
        }

        public void Track(SonarAnalysisContext context, params ObjectCreationCondition[] conditions)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSyntaxNodeActionInNonGenerated(
                            GeneratedCodeRecognizer,
                            TrackObjectCreation,
                            TrackedSyntaxKinds);
                    }
                });

            void TrackObjectCreation(SyntaxNodeAnalysisContext c)
            {
                if (IsTrackedObjectCreation(c.Node, c.SemanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetLocation()));
                }
            }

            bool IsTrackedObjectCreation(SyntaxNode objectCreationExpression, SemanticModel semanticModel)
            {
                var objectCreationContext = new ObjectCreationContext(objectCreationExpression, semanticModel);
                return conditions.All(c => c(objectCreationContext));
            }
        }

        internal ObjectCreationCondition FirstArgumentIs(KnownType type) =>
            (context) =>
                context.InvokedConstructorSymbol.Value != null &&
                context.InvokedConstructorSymbol.Value.ArgumentAtIndexIs(0, type);

        internal abstract ObjectCreationCondition FirstArgumentIsConstant();

        internal ObjectCreationCondition MatchConstructors(params KnownType[] types)
        {
            // We cannot do a syntax check first because a type name can be aliased with
            // a using Alias = Fully.Qualified.Name and we will generate false negative
            // for new Alias()
            return (context) =>
                context.InvokedConstructorSymbol.Value != null &&
                context.InvokedConstructorSymbol.Value.IsConstructor() &&
                context.InvokedConstructorSymbol.Value.ContainingType.IsAny(types);
        }
    }
}
