/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LossOfFractionInDivision : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2184";
        private const string MessageFormat = "Cast one of the operands of this division to '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var division = (BinaryExpressionSyntax)c.Node;

                    if (!(c.SemanticModel.GetSymbolInfo(division).Symbol is IMethodSymbol symbol)
                        || symbol.ContainingType == null
                        || !symbol.ContainingType.IsAny(KnownType.IntegralNumbersIncludingNative))
                    {
                        return;
                    }

                    if (TryGetTypeFromAssignmentToFloatType(division, c.SemanticModel, out var assignedToType)
                        || TryGetTypeFromArgumentMappedToFloatType(division, c.SemanticModel, out assignedToType)
                        || TryGetTypeFromReturnMappedToFloatType(division, c.SemanticModel, out assignedToType))
                    {
                        var diagnostic = Diagnostic.Create(Rule, division.GetLocation(), assignedToType.ToMinimalDisplayString(c.SemanticModel, division.SpanStart));
                        c.ReportIssue(diagnostic);
                    }
                },
                SyntaxKind.DivideExpression);

        private static bool TryGetTypeFromReturnMappedToFloatType(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol type)
        {
            if (division.Parent is ReturnStatementSyntax
                || division.Parent is LambdaExpressionSyntax)
            {
                type = (semanticModel.GetEnclosingSymbol(division.SpanStart) as IMethodSymbol)?.ReturnType;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            type = null;
            return false;
        }

        private static bool TryGetTypeFromArgumentMappedToFloatType(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol type)
        {
            if (division.Parent is not ArgumentSyntax argument)
            {
                type = null;
                return false;
            }

            if (argument.Parent.Parent is not InvocationExpressionSyntax invocation)
            {
                type = null;
                return false;
            }

            var lookup = new CSharpMethodParameterLookup(invocation, semanticModel);
            if (!lookup.TryGetSymbol(argument, out var parameter))
            {
                type = null;
                return false;
            }

            type = parameter.Type;
            return type.IsAny(KnownType.NonIntegralNumbers);
        }

        private static bool TryGetTypeFromAssignmentToFloatType(SyntaxNode division, SemanticModel semanticModel, out ITypeSymbol type)
        {
            if (division.Parent is AssignmentExpressionSyntax assignment)
            {
                type = semanticModel.GetTypeInfo(assignment.Left).Type;
                return type.IsAny(KnownType.NonIntegralNumbers);
            }

            if (division.Ancestors().Where(x => TupleExpressionSyntaxWrapper.IsInstance(x)).LastOrDefault() != null)
            {
                if (division.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault() is { } assignmentSyntax
                    && assignmentSyntax.MapAssignmentArguments() is { } assignmentMappings)
                {
                    var assignementLeft = assignmentMappings.Where(x => x.Right.Equals(division)).FirstOrDefault().Left;
                    if (assignementLeft != null)
                    {
                        type = semanticModel.GetTypeInfo(assignementLeft).Type;
                        return type.IsAny(KnownType.NonIntegralNumbers);
                    }
                }
                else if (division.Ancestors().OfType<VariableDeclarationSyntax>().FirstOrDefault() is { } variableDeclaration)
                {
                    var tupleArguments = ((TupleExpressionSyntaxWrapper)division.Parent.Parent).AllArguments();
                    var declarationType = semanticModel.GetTypeInfo(variableDeclaration.Type).Type;
                    var tupleTypes = (declarationType as INamedTypeSymbol)?.TupleElements().Select(x => x.Type).ToArray();
                    type = tupleTypes[tupleArguments.IndexOf((ArgumentSyntax)division.Parent)];
                    return type.IsAny(KnownType.NonIntegralNumbers);
                }
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
