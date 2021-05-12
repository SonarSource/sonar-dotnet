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
    public sealed class NewGuidShouldNotBeUsed : NewGuidShouldNotBeUsedBase<SyntaxKind>
    {
        internal const string DiagnosticId = "S4581";
        private const string MessageFormat = "Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this Guid instantiation.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var objectCreationSyntax = (ObjectCreationExpressionSyntax)c.Node;

                    if (objectCreationSyntax.ArgumentList?.Arguments.Count == 0 &&
                        c.SemanticModel.GetSymbolInfo(objectCreationSyntax).Symbol is IMethodSymbol methodSymbol &&
                        methodSymbol.ContainingType.Is(KnownType.System_Guid))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, objectCreationSyntax.GetLocation()));
                    }
                },
                SyntaxKind.ObjectCreationExpression);
        }
    }
}
