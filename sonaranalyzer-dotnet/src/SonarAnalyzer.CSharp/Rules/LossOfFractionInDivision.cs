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
    public sealed class LossOfFractionInDivision : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2184";
        private const string MessageFormat = "Cast one of the operands of this division to '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var division = (BinaryExpressionSyntax) c.Node;

                    if (!(c.SemanticModel.GetSymbolInfo(division).Symbol is IMethodSymbol symbol) ||
                        symbol.ContainingType == null ||
                        !symbol.ContainingType.IsAny(KnownType.IntegralNumbers))
                    {
                        return;
                    }

                    if (TryGetTypeFromAssignmentToFloatType(division, c.SemanticModel, out var assignedToType) ||
                        TryGetTypeFromArgumentMappedToFloatType(division, c.SemanticModel, out assignedToType) ||
                        TryGetTypeFromReturnMappedToFloatType(division, c.SemanticModel, out assignedToType))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(
                            rule,
                            division.GetLocation(),
                            assignedToType.ToMinimalDisplayString(c.SemanticModel, division.SpanStart)));
                    }
                },
                SyntaxKind.DivideExpression);
        }

        private static bool TryGetTypeFromReturnMappedToFloatType(BinaryExpressionSyntax division, SemanticModel semanticModel,
            out ITypeSymbol type)
        {
            if (division.Parent is ReturnStatementSyntax ||
                division.Parent is LambdaExpressionSyntax)
            {
                type = (semanticModel.GetEnclosingSymbol(division.SpanStart) as IMethodSymbol)?.ReturnType;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            type = null;
            return false;
        }

        private static bool TryGetTypeFromArgumentMappedToFloatType(BinaryExpressionSyntax division, SemanticModel semanticModel,
            out ITypeSymbol type)
        {
            if (!(division.Parent is ArgumentSyntax argument))
            {
                type = null;
                return false;
            }

            if (!(argument.Parent.Parent is InvocationExpressionSyntax invocation))
            {
                type = null;
                return false;
            }

            var lookup = new CSharpMethodParameterLookup(invocation, semanticModel);
            if (!lookup.TryGetParameterSymbol(argument, out var parameter))
            {
                type = null;
                return false;
            }

            type = parameter.Type;
            return type.IsAny(KnownType.NonIntegralNumbers);
        }

        private static bool TryGetTypeFromAssignmentToFloatType(BinaryExpressionSyntax division, SemanticModel semanticModel,
            out ITypeSymbol type)
        {
            if (division.Parent is AssignmentExpressionSyntax assignment)
            {
                type = semanticModel.GetTypeInfo(assignment.Left).Type;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            if (division.Parent.Parent.Parent is VariableDeclarationSyntax variableDecl)
            {
                type = semanticModel.GetTypeInfo(variableDecl.Type).Type;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            type = null;
            return false;
        }
    }
}
