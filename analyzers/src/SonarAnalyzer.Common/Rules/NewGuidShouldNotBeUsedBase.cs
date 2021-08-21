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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class NewGuidShouldNotBeUsedBase<TExpression, TSyntaxKind> : SonarDiagnosticAnalyzer
        where TExpression : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S4581";
        private const string MessageFormat = "Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this GUID instantiation.";
        protected readonly DiagnosticDescriptor rule;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract IEnumerable<TExpression> ArgumentExpressions(SyntaxNode node);

        protected NewGuidShouldNotBeUsedBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (NotAllowedGuidCtorArguments(c.Node, c.SemanticModel)
                        && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IMethodSymbol methodSymbol
                        && methodSymbol.ContainingType.Is(KnownType.System_Guid))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                    }
                },
                Language.SyntaxKind.ObjectCreationExpressions);

            context.RegisterSyntaxNodeActionInNonGenerated(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (c.SemanticModel.GetTypeInfo(c.Node).Type.Is(KnownType.System_Guid))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                    }
                },
                Language.SyntaxKind.DefaultLiteral);
        }

        private bool NotAllowedGuidCtorArguments(SyntaxNode ctorNode, SemanticModel semanticModel)
        {
            var arguments = ArgumentExpressions(ctorNode).ToArray();
            return arguments.Length == 0 || CreatesGuidEmpty(arguments, semanticModel);
        }

        private static bool CreatesGuidEmpty(TExpression[] arguments, SemanticModel semanticModel) =>
            arguments.Length == 1
            && semanticModel.GetConstantValue(arguments[0]) is { HasValue: true } optional
            && optional.Value is string str
            && Guid.TryParse(str, out var guid)
            && guid == Guid.Empty;
        }
}
