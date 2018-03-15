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

using System.Collections.Immutable;
using System.Linq;
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
    public sealed class ParameterValidationInAsyncShouldBeWrapped : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4457";
        private const string MessageFormat = "Split this method into two, one handling parameters check and the other " +
            "handling the iterator.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var awaitExpression = (AwaitExpressionSyntax)c.Node;

                    var methodDeclaration = awaitExpression.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                    if (methodDeclaration == null)
                    {
                        return;
                    }

                    var hasAnyArgumentException = methodDeclaration.DescendantNodes()
                        .OfType<ObjectCreationExpressionSyntax>()
                        .Select(objectCreationSyntax => objectCreationSyntax.Type)
                        .OfType<IdentifierNameSyntax>()
                        .Select(identifierSyntax => c.SemanticModel.GetSymbolInfo(identifierSyntax).Symbol)
                        .Any(symbol => (symbol.OriginalDefinition as ITypeSymbol).DerivesFrom(KnownType.System_ArgumentException));

                    if (hasAnyArgumentException)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.AwaitExpression);
        }
    }
}
