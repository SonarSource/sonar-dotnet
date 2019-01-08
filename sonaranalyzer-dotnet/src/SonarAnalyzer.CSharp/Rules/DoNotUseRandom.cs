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
    public sealed class DoNotUseRandom : HotspotDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2245";
        private const string MessageFormat = "Make sure that using this pseudorandom number generator is safe here.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                .WithNotConfigurable();

        public DoNotUseRandom()
            : base(AnalyzerConfiguration.Hotspot)
        {
        }

        public DoNotUseRandom(IAnalyzerConfiguration analyzerConfiguration)
            : base(analyzerConfiguration)
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                ccc =>
                {
                    if (!IsEnabled(ccc.Options))
                    {
                        return;
                    }

                    ccc.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var objectCreationSyntax = (ObjectCreationExpressionSyntax)c.Node;

                            var argumentsCount = objectCreationSyntax.ArgumentList?.Arguments.Count;

                            if (argumentsCount <= 1 && // Random has two ctors - with zero and one parameter
                                c.SemanticModel.GetSymbolInfo(objectCreationSyntax).Symbol is IMethodSymbol methodSymbol &&
                                methodSymbol.ContainingType.Is(KnownType.System_Random))
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, objectCreationSyntax.GetLocation()));
                            }
                        },
                        SyntaxKind.ObjectCreationExpression);
                });
        }
    }
}
